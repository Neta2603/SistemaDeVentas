using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SDV.Application.UseCases;

namespace SDV.WorkerService;

/// <summary>
/// Worker Service que ejecuta el proceso ETL completo
/// Fase E: Extracción de datos a Staging
/// Fase L: Carga de dimensiones del Data Warehouse
/// </summary>
public class EtlWorker : BackgroundService
{
    private readonly ILogger<EtlWorker> _logger;
    private readonly ExtractDataUseCase _extractDataUseCase;
    private readonly LoadDimensionsUseCase _loadDimensionsUseCase;
    private readonly IHostApplicationLifetime _lifetime;

    public EtlWorker(
        ILogger<EtlWorker> logger,
        ExtractDataUseCase extractDataUseCase,
        LoadDimensionsUseCase loadDimensionsUseCase,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _extractDataUseCase = extractDataUseCase;
        _loadDimensionsUseCase = loadDimensionsUseCase;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service ETL iniciado en: {Time}", DateTimeOffset.Now);
        var totalStartTime = DateTime.Now;

        try
        {
            // ==================== FASE E: EXTRACCIÓN ====================
            _logger.LogInformation("");
            _logger.LogInformation("╔═══════════════════════════════════════════════════════╗");
            _logger.LogInformation("║         FASE 1: EXTRACCIÓN (E) - STAGING             ║");
            _logger.LogInformation("╚═══════════════════════════════════════════════════════╝");
            
            var extractionResult = await _extractDataUseCase.ExecuteAsync();

            // Mostrar resumen de extracción
            _logger.LogInformation("════════════════════════════════════════");
            _logger.LogInformation("       RESUMEN DE EXTRACCIÓN ETL        ");
            _logger.LogInformation("════════════════════════════════════════");
            _logger.LogInformation("✓ Clientes extraídos:  {Count}", extractionResult.CustomersExtracted);
            _logger.LogInformation("✓ Productos extraídos: {Count}", extractionResult.ProductsExtracted);
            _logger.LogInformation("✓ Órdenes extraídas:   {Count}", extractionResult.OrdersExtracted);
            _logger.LogInformation("⏱ Tiempo de ejecución: {Time}", extractionResult.ExecutionTime);
            _logger.LogInformation("════════════════════════════════════════");

            if (!extractionResult.Success)
            {
                _logger.LogError("La extracción finalizó con errores: {Error}", extractionResult.ErrorMessage);
                return;
            }

            // ==================== FASE L: CARGA DE DIMENSIONES ====================
            _logger.LogInformation("");
            _logger.LogInformation("╔═══════════════════════════════════════════════════════╗");
            _logger.LogInformation("║       FASE 2: CARGA (L) - DIMENSIONES DW             ║");
            _logger.LogInformation("╚═══════════════════════════════════════════════════════╝");
            
            var loadResult = await _loadDimensionsUseCase.ExecuteAsync();

            // ==================== RESUMEN FINAL ====================
            var totalTime = DateTime.Now - totalStartTime;
            
            _logger.LogInformation("");
            _logger.LogInformation("╔═══════════════════════════════════════════════════════╗");
            _logger.LogInformation("║              PROCESO ETL COMPLETADO                  ║");
            _logger.LogInformation("╚═══════════════════════════════════════════════════════╝");
            _logger.LogInformation("┌─────────────────────────────────────────────────────┐");
            _logger.LogInformation("│ EXTRACCIÓN (Staging)                                │");
            _logger.LogInformation("│   • Clientes:  {Count,-10}                         │", extractionResult.CustomersExtracted);
            _logger.LogInformation("│   • Productos: {Count,-10}                         │", extractionResult.ProductsExtracted);
            _logger.LogInformation("│   • Órdenes:   {Count,-10}                         │", extractionResult.OrdersExtracted);
            _logger.LogInformation("├─────────────────────────────────────────────────────┤");
            _logger.LogInformation("│ CARGA DIMENSIONES (Data Warehouse)                  │");
            _logger.LogInformation("│   • Total procesados:  {Count,-10}                 │", loadResult.TotalRecordsProcessed);
            _logger.LogInformation("│   • Total insertados:  {Count,-10}                 │", loadResult.TotalRecordsInserted);
            _logger.LogInformation("│   • Total actualizados:{Count,-10}                 │", loadResult.TotalRecordsUpdated);
            _logger.LogInformation("└─────────────────────────────────────────────────────┘");
            _logger.LogInformation("⏱ TIEMPO TOTAL DE EJECUCIÓN: {Time}", totalTime);
            _logger.LogInformation("═══════════════════════════════════════════════════════");

            if (!loadResult.Success)
            {
                _logger.LogWarning("La carga de dimensiones finalizó con advertencias");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error crítico durante la ejecución del Worker Service");
        }
        finally
        {
            _logger.LogInformation("Deteniendo Worker Service...");
            _lifetime.StopApplication();
        }
    }
}
