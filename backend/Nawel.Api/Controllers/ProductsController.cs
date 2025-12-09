using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nawel.Api.DTOs;
using Nawel.Api.Services.ProductInfo;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

/// <summary>
/// Contrôleur pour l'extraction d'informations de produits depuis des URLs externes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductInfoExtractor _productInfoExtractor;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductInfoExtractor productInfoExtractor, ILogger<ProductsController> logger)
    {
        _productInfoExtractor = productInfoExtractor;
        _logger = logger;
    }

    /// <summary>
    /// Extrait les informations d'un produit (nom, prix, image) depuis une URL via l'API OpenGraph.
    /// </summary>
    /// <param name="request">L'URL du produit à analyser.</param>
    /// <param name="cancellationToken">Token d'annulation de la requête.</param>
    /// <returns>Les informations extraites du produit (nom, prix, image, description).</returns>
    /// <response code="200">Informations extraites avec succès.</response>
    /// <response code="400">URL invalide ou modèle de requête incorrect.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Impossible d'extraire les informations depuis cette URL.</response>
    /// <response code="500">Erreur serveur lors de l'extraction.</response>
    /// <remarks>
    /// Cette méthode utilise l'API OpenGraph (opengraph.io) pour extraire les métadonnées.
    /// Les requêtes sont suivies dans la table OpenGraphRequests pour monitoring.
    /// </remarks>
    [HttpPost("extract-info")]
    [ProducesResponseType(typeof(ProductInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductInfoDto>> ExtractProductInfo([FromBody] ExtractProductInfoRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var productInfo = await _productInfoExtractor.ExtractProductInfoAsync(request.Url, currentUserId, cancellationToken);

            if (productInfo == null)
            {
                return NotFound(new { message = "Impossible d'extraire les informations du produit depuis cette URL." });
            }

            return Ok(productInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting product info from URL: {Url}", request.Url);
            return StatusCode(500, new { message = "Une erreur s'est produite lors de l'extraction des informations." });
        }
    }
}
