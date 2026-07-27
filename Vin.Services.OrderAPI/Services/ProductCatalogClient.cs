using System.Text.Json;

namespace Vin.Services.OrderAPI.Services;

public interface IProductCatalogClient
{
    Task<IReadOnlyDictionary<int, ProductSnapshot>> GetProductsAsync(IEnumerable<int> productIds, CancellationToken cancellationToken);
}

public sealed record ProductSnapshot(int ProductId, string Name, decimal Price);

public sealed class ProductCatalogClient : IProductCatalogClient
{
    private readonly HttpClient _client;

    public ProductCatalogClient(HttpClient client) => _client = client;

    public async Task<IReadOnlyDictionary<int, ProductSnapshot>> GetProductsAsync(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, ProductSnapshot>();
        foreach (var productId in productIds.Distinct())
        {
            using var response = await _client.GetAsync($"api/Product/GetProductById/{productId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
            if (!document.RootElement.TryGetProperty("isSuccess", out var success) || !success.GetBoolean() ||
                !document.RootElement.TryGetProperty("result", out var item))
            {
                throw new KeyNotFoundException($"Product {productId} was not found.");
            }

            var snapshot = item.Deserialize<ProductSnapshot>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (snapshot is null)
            {
                throw new KeyNotFoundException($"Product {productId} was not found.");
            }
            result[productId] = snapshot;
        }

        return result;
    }
}
