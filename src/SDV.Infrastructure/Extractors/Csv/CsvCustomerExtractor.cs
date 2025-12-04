using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;
using System.Globalization;

namespace SDV.Infrastructure.Extractors.Csv;

/// <summary>
/// Extractor de clientes desde archivo CSV
/// </summary>
public class CsvCustomerExtractor : IDataExtractor<StagingCustomer>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CsvCustomerExtractor> _logger;

    public string ExtractorName => "CSV - Customers";

    public CsvCustomerExtractor(IConfiguration configuration, ILogger<CsvCustomerExtractor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<StagingCustomer>> ExtractAsync()
    {
        var relativePath = _configuration["DataSources:Csv:CustomersPath"] ?? "data/customers.csv";
        
        // Construir ruta absoluta desde la raíz del proyecto
        var projectRoot = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(projectRoot, "..", "..", relativePath);
        filePath = Path.GetFullPath(filePath);
        
        _logger.LogInformation("Leyendo archivo CSV de clientes: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogError("Archivo CSV de clientes no encontrado: {FilePath}", filePath);
            throw new FileNotFoundException($"Archivo CSV no encontrado: {filePath}");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<CsvCustomerRecord>().ToList();
        
        var customers = records.Select(r => new StagingCustomer
        {
            CustomerID = r.CustomerID,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Email = r.Email,
            Phone = r.Phone,
            City = r.City,
            Country = r.Country,
            LoadDate = DateTime.Now
        }).ToList();

        _logger.LogInformation("Extraídos {Count} clientes desde CSV", customers.Count);
        
        return await Task.FromResult(customers);
    }

    private class CsvCustomerRecord
    {
        public int CustomerID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}