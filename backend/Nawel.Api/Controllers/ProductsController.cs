using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nawel.Api.DTOs;
using Nawel.Api.Services.ProductInfo;
using System.Security.Claims;

namespace Nawel.Api.Controllers;

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

    [HttpPost("extract-info")]
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
