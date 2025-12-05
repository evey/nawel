using Nawel.Api.DTOs;

namespace Nawel.Api.Services.ProductInfo;

public interface IProductInfoExtractor
{
    Task<ProductInfoDto?> ExtractProductInfoAsync(string url, int userId, CancellationToken cancellationToken = default);
}
