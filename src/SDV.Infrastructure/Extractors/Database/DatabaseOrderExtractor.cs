using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;

namespace SDV.Infrastructure.Extractors.Database;

/// <summary>
/// Extractor de órdenes desde base de datos externa (Mock por ahora)
/// </summary>
public class DatabaseOrderExtractor : IDataExtractor<StagingOrder>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseOrderExtractor> _logger;

    public string ExtractorName => "Database - Orders";

    public DatabaseOrderExtractor(IConfiguration configuration, ILogger<DatabaseOrderExtractor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<StagingOrder>> ExtractAsync()
    {
        _logger.LogInformation("Extrayendo órdenes desde base de datos externa (Mock)");

        try
        {
            var mockOrders = GenerateMockOrders();
            
            _logger.LogInformation("Extraídas {Count} órdenes desde base de datos", mockOrders.Count);
            
            return await Task.FromResult(mockOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo órdenes desde base de datos");
            throw;
        }
    }

    private List<StagingOrder> GenerateMockOrders()
    {
        var statuses = new[] { "Pending", "Shipped", "Delivered", "Cancelled" };
        var random = new Random(42);
        var orders = new List<StagingOrder>();

        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var dateRange = (endDate - startDate).Days;

        // Genera 100 órdenes con OrderIDs del 1 al 100
        // IMPORTANTE: Los CSVs de order_details deben tener OrderIDs en este rango
        int numberOfOrders = 100;

        for (int i = 1; i <= numberOfOrders; i++)
        {
            orders.Add(new StagingOrder
            {
                OrderID = i,
                CustomerID = random.Next(1, 5001),
                OrderDate = startDate.AddDays(random.Next(dateRange)),
                Status = statuses[random.Next(statuses.Length)],
                LoadDate = DateTime.Now
            });
        }

        _logger.LogInformation("Generadas {Count} órdenes mock (OrderIDs: 1-{Max})", orders.Count, numberOfOrders);
        return orders;
    }
}