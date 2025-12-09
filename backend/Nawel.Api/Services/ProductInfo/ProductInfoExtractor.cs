using HtmlAgilityPack;
using Nawel.Api.Data;
using Nawel.Api.DTOs;
using Nawel.Api.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Nawel.Api.Services.ProductInfo;

public class ProductInfoExtractor : IProductInfoExtractor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductInfoExtractor> _logger;
    private readonly IConfiguration _configuration;
    private readonly NawelDbContext _context;

    public ProductInfoExtractor(
        IHttpClientFactory httpClientFactory,
        ILogger<ProductInfoExtractor> logger,
        IConfiguration configuration,
        NawelDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _context = context;
    }

    public async Task<ProductInfoDto?> ExtractProductInfoAsync(string url, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Extracting product info from URL: {Url}", url);

            // Détecter si c'est Amazon
            var isAmazon = url.Contains("amazon.", StringComparison.OrdinalIgnoreCase);

            // Créer le client HTTP avec des headers réalistes
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Headers plus complets pour ressembler à un vrai navigateur
            // Note: Accept-Encoding est géré automatiquement par HttpClient pour la décompression
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            request.Headers.Add("Accept-Language", "fr-FR,fr;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("DNT", "1");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Add("Sec-Fetch-Dest", "document");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "none");
            request.Headers.Add("Sec-Fetch-User", "?1");
            request.Headers.Add("Cache-Control", "max-age=0");

            if (isAmazon)
            {
                // Headers spécifiques Amazon
                request.Headers.Add("Sec-Ch-Ua", "\"Chromium\";v=\"122\", \"Not(A:Brand\";v=\"24\", \"Google Chrome\";v=\"122\"");
                request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
                request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");

                // Ajouter des cookies de session basiques
                request.Headers.Add("Cookie", "session-id=000-0000000-0000000; ubid-main=000-0000000-0000000");
            }

            var response = await httpClient.SendAsync(request, cancellationToken);

            // Pour Amazon, si on a un 404 ou 503, c'est probablement un blocage
            if (isAmazon && (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                             response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable))
            {
                _logger.LogWarning("Amazon returned {StatusCode}, trying alternative approach", response.StatusCode);
                return null; // On retourne null pour permettre un remplissage manuel
            }

            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parser le HTML
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var productInfo = new ProductInfoDto();

            // Extraire les informations depuis les meta tags Open Graph et autres
            productInfo.Name = ExtractName(htmlDoc);

            // Pour Temu, essayer d'extraire l'image depuis les paramètres de l'URL
            if (url.Contains("temu.com", StringComparison.OrdinalIgnoreCase))
            {
                productInfo.ImageUrl = ExtractTemuImageFromUrl(url);
            }

            // Si pas d'image trouvée, utiliser l'extraction standard
            if (string.IsNullOrEmpty(productInfo.ImageUrl))
            {
                productInfo.ImageUrl = ExtractImageUrl(htmlDoc, url);
            }

            productInfo.Description = ExtractDescription(htmlDoc);

            var priceInfo = ExtractPrice(htmlDoc);
            productInfo.Price = priceInfo.Price;
            productInfo.Currency = priceInfo.Currency;

            _logger.LogInformation("Extracted product info: Name={Name}, Price={Price}, Currency={Currency}, HasImage={HasImage}",
                productInfo.Name, productInfo.Price, productInfo.Currency, !string.IsNullOrEmpty(productInfo.ImageUrl));

            // Si les informations sont incomplètes et OpenGraph.io est activé, l'utiliser comme fallback
            if (IsDataIncomplete(productInfo) && IsOpenGraphEnabled())
            {
                _logger.LogInformation("Extraction incomplete, trying OpenGraph.io fallback for URL: {Url}", url);
                var openGraphResult = await TryExtractWithOpenGraphAsync(url, userId, cancellationToken);
                if (openGraphResult != null)
                {
                    // Compléter les données manquantes
                    productInfo.Name ??= openGraphResult.Name;
                    productInfo.ImageUrl ??= openGraphResult.ImageUrl;
                    productInfo.Description ??= openGraphResult.Description;
                    productInfo.Price ??= openGraphResult.Price;
                    productInfo.Currency ??= openGraphResult.Currency;

                    _logger.LogInformation("OpenGraph.io enriched data: Name={Name}, Price={Price}, HasImage={HasImage}",
                        productInfo.Name, productInfo.Price, !string.IsNullOrEmpty(productInfo.ImageUrl));
                }
            }

            return productInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting product info from URL: {Url}", url);

            // En cas d'erreur, essayer OpenGraph.io si activé
            if (IsOpenGraphEnabled())
            {
                try
                {
                    _logger.LogInformation("Trying OpenGraph.io as last resort for URL: {Url}", url);
                    return await TryExtractWithOpenGraphAsync(url, userId, cancellationToken);
                }
                catch (Exception ogEx)
                {
                    _logger.LogError(ogEx, "OpenGraph.io fallback also failed");
                }
            }

            return null;
        }
    }

    private string? ExtractName(HtmlDocument doc)
    {
        // Ordre de priorité pour trouver le nom du produit
        var selectors = new[]
        {
            // Sélecteurs spécifiques Amazon
            "//span[@id='productTitle']",
            "//h1[@id='title']/span",
            // Sélecteurs Open Graph standards
            "//meta[@property='og:title']/@content",
            "//meta[@name='twitter:title']/@content",
            "//meta[@property='product:title']/@content",
            "//h1[@id='title']",
            "//h1[contains(@class, 'product')]",
            "//h1",
            "//title"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            var value = node?.GetAttributeValue("content", string.Empty) ?? node?.InnerText ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(value))
            {
                // Nettoyer le texte
                value = HtmlEntity.DeEntitize(value).Trim();

                // Si c'est un title, enlever le nom du site (souvent après " - " ou " | ")
                if (selector.Contains("title"))
                {
                    var separators = new[] { " - ", " | ", " – " };
                    foreach (var sep in separators)
                    {
                        var parts = value.Split(sep);
                        if (parts.Length > 1 && parts[0].Length > 10)
                        {
                            return parts[0].Trim();
                        }
                    }
                }

                return value;
            }
        }

        return null;
    }

    private string? ExtractImageUrl(HtmlDocument doc, string baseUrl)
    {
        var selectors = new[]
        {
            // Sélecteurs Open Graph standards (priorité haute)
            "//meta[@property='og:image']/@content",
            "//meta[@property='og:image:url']/@content",
            "//meta[@name='twitter:image']/@content",
            // Sélecteurs spécifiques Amazon
            "//img[@id='landingImage']",
            "//img[@id='imgBlkFront']",
            "//div[@id='imgTagWrapperId']//img",
            // Sélecteurs génériques pour images produits
            "//img[contains(@class, 'product-image')]",
            "//img[contains(@class, 'product')]",
            "//img[contains(@class, 'main-image')]",
            "//img[contains(@class, 'main')]",
            "//img[contains(@id, 'product')]",
            "//img[contains(@alt, 'product')]",
            // Fallback : première image dans un conteneur produit
            "//div[contains(@class, 'product-images')]//img[1]",
            "//div[contains(@class, 'product-gallery')]//img[1]"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node == null) continue;

            // Essayer différents attributs dans l'ordre
            var value = node.GetAttributeValue("content", string.Empty);
            if (string.IsNullOrWhiteSpace(value))
                value = node.GetAttributeValue("data-old-hires", string.Empty);
            if (string.IsNullOrWhiteSpace(value))
                value = node.GetAttributeValue("data-src", string.Empty);
            if (string.IsNullOrWhiteSpace(value))
                value = node.GetAttributeValue("data-lazy-src", string.Empty);
            if (string.IsNullOrWhiteSpace(value))
                value = node.GetAttributeValue("data-zoom-src", string.Empty);
            if (string.IsNullOrWhiteSpace(value))
                value = node.GetAttributeValue("src", string.Empty);

            if (!string.IsNullOrWhiteSpace(value))
            {
                // Convertir en URL absolue si nécessaire
                if (value.StartsWith("//"))
                {
                    value = "https:" + value;
                }
                else if (value.StartsWith("/"))
                {
                    var uri = new Uri(baseUrl);
                    value = $"{uri.Scheme}://{uri.Host}{value}";
                }

                // Vérifier que c'est une URL valide
                if (Uri.TryCreate(value, UriKind.Absolute, out var imageUri) &&
                    (imageUri.Scheme == Uri.UriSchemeHttp || imageUri.Scheme == Uri.UriSchemeHttps))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private string? ExtractDescription(HtmlDocument doc)
    {
        var selectors = new[]
        {
            "//meta[@property='og:description']/@content",
            "//meta[@name='description']/@content",
            "//meta[@name='twitter:description']/@content",
            "//meta[@property='product:description']/@content"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            var value = node?.GetAttributeValue("content", string.Empty) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(value))
            {
                value = HtmlEntity.DeEntitize(value).Trim();

                // Limiter à 500 caractères
                if (value.Length > 500)
                {
                    value = value.Substring(0, 497) + "...";
                }

                return value;
            }
        }

        return null;
    }

    private (decimal? Price, string? Currency) ExtractPrice(HtmlDocument doc)
    {
        // Essayer d'abord les meta tags structurés
        var priceNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:price:amount']/@content") ??
                        doc.DocumentNode.SelectSingleNode("//meta[@property='product:price:amount']/@content");

        var currencyNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:price:currency']/@content") ??
                           doc.DocumentNode.SelectSingleNode("//meta[@property='product:price:currency']/@content");

        var priceText = priceNode?.GetAttributeValue("content", string.Empty) ?? string.Empty;
        var currency = currencyNode?.GetAttributeValue("content", string.Empty) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(priceText))
        {
            if (TryParsePrice(priceText, out var price))
            {
                return (price, currency ?? "EUR");
            }
        }

        // Si pas trouvé dans les meta tags, chercher dans le HTML
        var priceSelectors = new[]
        {
            // Sélecteurs spécifiques Amazon
            "//span[@class='a-price-whole']",
            "//span[contains(@class, 'a-price-whole')]",
            "//span[@id='priceblock_ourprice']",
            "//span[@id='priceblock_dealprice']",
            "//span[contains(@class, 'a-price')]/span[@class='a-offscreen']",
            "//span[@id='price_inside_buybox']",
            // Sélecteurs génériques
            "//span[contains(@class, 'price')]",
            "//div[contains(@class, 'price')]",
            "//span[@id='price']"
        };

        foreach (var selector in priceSelectors)
        {
            var nodes = doc.DocumentNode.SelectNodes(selector);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var text = HtmlEntity.DeEntitize(node.InnerText).Trim();

                    if (TryParsePrice(text, out var price))
                    {
                        // Détecter la devise depuis le texte
                        currency = DetectCurrency(text);
                        return (price, currency);
                    }
                }
            }
        }

        return (null, null);
    }

    private bool TryParsePrice(string text, out decimal price)
    {
        price = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Nettoyer le texte : garder seulement chiffres, virgule, point et espace
        var cleaned = Regex.Replace(text, @"[^\d,.\s]", "");
        cleaned = cleaned.Trim();

        // Remplacer les espaces (séparateurs de milliers français)
        cleaned = cleaned.Replace(" ", "");

        // Déterminer si c'est format français (virgule) ou anglais (point)
        var hasComma = cleaned.Contains(',');
        var hasDot = cleaned.Contains('.');

        if (hasComma && hasDot)
        {
            // Les deux présents : le dernier est le séparateur décimal
            var lastComma = cleaned.LastIndexOf(',');
            var lastDot = cleaned.LastIndexOf('.');

            if (lastComma > lastDot)
            {
                // Format français : 1.234,56
                cleaned = cleaned.Replace(".", "").Replace(",", ".");
            }
            else
            {
                // Format anglais : 1,234.56
                cleaned = cleaned.Replace(",", "");
            }
        }
        else if (hasComma)
        {
            // Seulement virgule : vérifier si c'est séparateur décimal ou milliers
            var parts = cleaned.Split(',');
            if (parts.Length == 2 && parts[1].Length <= 2)
            {
                // C'est un séparateur décimal : 12,99
                cleaned = cleaned.Replace(",", ".");
            }
            else
            {
                // C'est un séparateur de milliers : 1,234
                cleaned = cleaned.Replace(",", "");
            }
        }

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
    }

    private string DetectCurrency(string text)
    {
        if (text.Contains("€") || text.Contains("EUR"))
            return "EUR";
        if (text.Contains("$") || text.Contains("USD"))
            return "USD";
        if (text.Contains("£") || text.Contains("GBP"))
            return "GBP";
        if (text.Contains("CHF"))
            return "CHF";

        return "EUR"; // Par défaut
    }

    private string? ExtractTemuImageFromUrl(string url)
    {
        try
        {
            // Temu met l'URL de l'image dans le paramètre "top_gallery_url"
            var uri = new Uri(url);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var imageUrl = queryParams["top_gallery_url"];

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                // Décoder l'URL si elle est encodée
                imageUrl = Uri.UnescapeDataString(imageUrl);

                // Vérifier que c'est une URL valide
                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var validUri) &&
                    (validUri.Scheme == Uri.UriSchemeHttp || validUri.Scheme == Uri.UriSchemeHttps))
                {
                    _logger.LogInformation("Extracted Temu image from URL parameter: {ImageUrl}", imageUrl);
                    return imageUrl;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract Temu image from URL parameters");
        }

        return null;
    }

    private bool IsOpenGraphEnabled()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENGRAPH_API_KEY")
            ?? _configuration["OpenGraph:ApiKey"];

        return _configuration.GetValue<bool>("OpenGraph:Enabled") &&
               !string.IsNullOrWhiteSpace(apiKey);
    }

    private bool IsDataIncomplete(ProductInfoDto productInfo)
    {
        // Considérer les données incomplètes si au moins l'image OU le nom manque
        return string.IsNullOrEmpty(productInfo.Name) || string.IsNullOrEmpty(productInfo.ImageUrl);
    }

    private async Task<ProductInfoDto?> TryExtractWithOpenGraphAsync(string url, int userId, CancellationToken cancellationToken)
    {
        bool success = false;
        string? errorMessage = null;

        try
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENGRAPH_API_KEY")
                ?? _configuration["OpenGraph:ApiKey"];

            var apiUrl = $"https://opengraph.io/api/1.1/site/{Uri.EscapeDataString(url)}?app_id={apiKey}";

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(apiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var ogData = JsonSerializer.Deserialize<JsonElement>(json);

            if (!ogData.TryGetProperty("hybridGraph", out var hybridGraph))
            {
                return null;
            }

            var productInfo = new ProductInfoDto();

            // Extraire le titre
            if (hybridGraph.TryGetProperty("title", out var title))
            {
                productInfo.Name = title.GetString();
            }

            // Extraire l'image
            if (hybridGraph.TryGetProperty("image", out var image) && image.ValueKind == JsonValueKind.String)
            {
                productInfo.ImageUrl = image.GetString();
            }

            // Extraire la description
            if (hybridGraph.TryGetProperty("description", out var description))
            {
                productInfo.Description = description.GetString();
            }

            // Extraire le prix si disponible
            if (hybridGraph.TryGetProperty("price_amount", out var priceAmount))
            {
                if (decimal.TryParse(priceAmount.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    productInfo.Price = price;
                }
            }

            if (hybridGraph.TryGetProperty("price_currency", out var priceCurrency))
            {
                productInfo.Currency = priceCurrency.GetString();
            }

            success = true;
            return productInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract data from OpenGraph.io");
            errorMessage = ex.Message;
            return null;
        }
        finally
        {
            // Track the OpenGraph.io API call
            try
            {
                var ogRequest = new OpenGraphRequest
                {
                    Url = url,
                    UserId = userId,
                    Success = success,
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OpenGraphRequests.Add(ogRequest);
                await _context.SaveChangesAsync();
            }
            catch (Exception trackEx)
            {
                _logger.LogError(trackEx, "Failed to track OpenGraph.io request");
            }
        }
    }
}
