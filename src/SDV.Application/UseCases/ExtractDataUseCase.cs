using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;

namespace SDV.Application.UseCases;

/// <summary>
/// Caso de uso para orquestar la extracción de datos desde múltiples fuentes
/// </summary>
public class ExtractDataUseCase
{
    private readonly IDataExtractor<StagingCustomer> _customerExtractor;
    private readonly IDataExtractor<StagingProduct> _productExtractor;
    private readonly IDataExtractor<StagingOrder> _orderExtractor;
    private readonly IDataExtractor<StagingOrderDetail> _orderDetailExtractor;
    private readonly IStagingRepository _stagingRepository;
    private readonly ILogger<ExtractDataUseCase> _logger;

    public ExtractDataUseCase(
        IDataExtractor<StagingCustomer> customerExtractor,
        IDataExtractor<StagingProduct> productExtractor,
        IDataExtractor<StagingOrder> orderExtractor,
        IDataExtractor<StagingOrderDetail> orderDetailExtractor,
        IStagingRepository stagingRepository,
        ILogger<ExtractDataUseCase> logger)
    {
        _customerExtractor = customerExtractor;
        _productExtractor = productExtractor;
        _orderExtractor = orderExtractor;
        _orderDetailExtractor = orderDetailExtractor;
        _stagingRepository = stagingRepository;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el proceso completo de extracción de datos
    /// </summary>
    public async Task<ExtractionResult> ExecuteAsync()
    {
        var result = new ExtractionResult();
        var startTime = DateTime.Now;

        _logger.LogInformation("=== INICIANDO PROCESO DE EXTRACCIÓN ETL ===");

        try
        {
            // 0. Limpiar tablas staging antes de extraer
            _logger.LogInformation("Limpiando tablas staging...");
            await _stagingRepository.CleanupStagingTablesAsync();
            _logger.LogInformation("✓ Tablas staging limpiadas");
            _logger.LogInformation("");
            // 1. Extraer Clientes
            _logger.LogInformation("Extrayendo clientes desde {Source}...", _customerExtractor.ExtractorName);
            var customers = await _customerExtractor.ExtractAsync();
            var customerList = customers.ToList();
            
            if (customerList.Any())
            {
                await _stagingRepository.BulkInsertCustomersAsync(customerList);
                result.CustomersExtracted = customerList.Count;
                _logger.LogInformation("✓ {Count} clientes extraídos correctamente", customerList.Count);
            }

            // 2. Extraer Productos
            _logger.LogInformation("Extrayendo productos desde {Source}...", _productExtractor.ExtractorName);
            var products = await _productExtractor.ExtractAsync();
            var productList = products.ToList();
            
            if (productList.Any())
            {
                await _stagingRepository.BulkInsertProductsAsync(productList);
                result.ProductsExtracted = productList.Count;
                _logger.LogInformation("✓ {Count} productos extraídos correctamente", productList.Count);
            }

            // 3. Extraer Órdenes
            _logger.LogInformation("Extrayendo órdenes desde {Source}...", _orderExtractor.ExtractorName);
            var orders = await _orderExtractor.ExtractAsync();
            var orderList = orders.ToList();
            
            if (orderList.Any())
            {
                await _stagingRepository.BulkInsertOrdersAsync(orderList);
                result.OrdersExtracted = orderList.Count;
                _logger.LogInformation("✓ {Count} órdenes extraídas correctamente", orderList.Count);
            }

            // 4. Extraer Detalles de Órdenes
            _logger.LogInformation("Extrayendo detalles de órdenes desde CSV...");
            var orderDetails = await _orderDetailExtractor.ExtractAsync();
            var orderDetailList = orderDetails.ToList();
            
            if (orderDetailList.Any())
            {
                await _stagingRepository.BulkInsertOrderDetailsAsync(orderDetailList);
                result.OrderDetailsExtracted = orderDetailList.Count;
                _logger.LogInformation("✓ {Count} detalles de órdenes extraídos correctamente", orderDetailList.Count);
            }

            result.Success = true;
            result.ExecutionTime = DateTime.Now - startTime;
            
            _logger.LogInformation("=== EXTRACCIÓN COMPLETADA EXITOSAMENTE ===");
            _logger.LogInformation("Tiempo de ejecución: {Time}", result.ExecutionTime);
            
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTime.Now - startTime;
            
            _logger.LogError(ex, "❌ Error durante el proceso de extracción");
            throw;
        }
    }
}

/// <summary>
/// Resultado del proceso de extracción
/// </summary>
public class ExtractionResult
{
    public bool Success { get; set; }
    public int CustomersExtracted { get; set; }
    public int ProductsExtracted { get; set; }
    public int OrdersExtracted { get; set; }
    public int OrderDetailsExtracted { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public string? ErrorMessage { get; set; }
}