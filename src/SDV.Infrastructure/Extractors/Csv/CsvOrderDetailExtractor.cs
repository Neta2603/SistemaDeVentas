using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;
using System.Globalization;

namespace SDV.Infrastructure.Extractors.Csv;

/// <summary>
/// Extractor de detalles de órdenes desde archivo CSV
/// Fuente: data/order_details.csv
/// </summary>
public class CsvOrderDetailExtractor : IDataExtractor<StagingOrderDetail>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CsvOrderDetailExtractor> _logger;

    public string ExtractorName => "CSV - Order Details";

    public CsvOrderDetailExtractor(IConfiguration configuration, ILogger<CsvOrderDetailExtractor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<StagingOrderDetail>> ExtractAsync()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "../../../../../data/order_details.csv");
        
        _logger.LogInformation("Extrayendo detalles de órdenes desde CSV: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogError("Archivo CSV de detalles de órdenes no encontrado: {FilePath}", filePath);
            return Enumerable.Empty<StagingOrderDetail>();
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = new List<StagingOrderDetail>();

        try
        {
            await foreach (var record in csv.GetRecordsAsync<OrderDetailCsvRecord>())
            {
                var stagingRecord = new StagingOrderDetail
                {
                    OrderID = record.OrderID,
                    ProductID = record.ProductID,
                    Quantity = record.Quantity,
                    TotalPrice = record.TotalPrice,
                    LoadDate = DateTime.Now
                };

                records.Add(stagingRecord);
            }

            _logger.LogInformation("✓ Extraídos {Count} detalles de órdenes desde CSV", records.Count);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer el archivo CSV de detalles de órdenes");
            return Enumerable.Empty<StagingOrderDetail>();
        }
    }
}

/// <summary>
/// Modelo para mapear registros del CSV de detalles de órdenes
/// </summary>
public class OrderDetailCsvRecord
{
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
