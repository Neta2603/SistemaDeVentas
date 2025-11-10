using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;

namespace SDV.Infrastructure.Extractors.Api;

/// <summary>
/// Extractor de productos desde API REST (Mock)
/// </summary>
public class ApiProductExtractor : IDataExtractor<StagingProduct>
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiProductExtractor> _logger;

    public string ExtractorName => "API REST - Products";

    public ApiProductExtractor(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<ApiProductExtractor> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<StagingProduct>> ExtractAsync()
    {
        _logger.LogInformation("Extrayendo productos desde API (Mock)");

        try
        {
            // Por ahora, retornamos datos mock hasta que tengamos una API real
            var mockProducts = GenerateMockProducts();
            
            _logger.LogInformation("Extraídos {Count} productos desde API (Mock)", mockProducts.Count);
            
            return await Task.FromResult(mockProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo productos desde API");
            throw;
        }
    }

    private List<StagingProduct> GenerateMockProducts()
    {
        var categories = new[] { "Electronics", "Clothing", "Food", "Books", "Sports" };
        var productNames = new[]
        {
            "Laptop", "Mouse", "Keyboard", "Monitor", "Headphones",
            "T-Shirt", "Jeans", "Sneakers", "Jacket", "Hat",
            "Coffee", "Tea", "Sugar", "Rice", "Pasta",
            "Novel", "Magazine", "Comics", "Textbook", "Dictionary",
            "Soccer Ball", "Tennis Racket", "Basketball", "Yoga Mat", "Dumbbells"
        };

        var random = new Random(42);
        var products = new List<StagingProduct>();

        for (int i = 1; i <= 50; i++)
        {
            products.Add(new StagingProduct
            {
                ProductID = i,
                ProductName = productNames[i % productNames.Length],
                Category = categories[i % categories.Length],
                Price = Math.Round((decimal)(random.NextDouble() * 500 + 10), 2),
                Stock = random.Next(0, 1000),
                LoadDate = DateTime.Now
            });
        }

        return products;
    }
}