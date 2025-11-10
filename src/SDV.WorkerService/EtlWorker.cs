using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SDV.Application.UseCases;

namespace SDV.WorkerService;

/// <summary>
/// Worker Service que ejecuta el proceso ETL de extracción
/// </summary>
public class EtlWorker : BackgroundService
{
    private readonly ILogger<EtlWorker> _logger;
    private readonly ExtractDataUseCase _extractDataUseCase;
    private readonly IHostApplicationLifetime _lifetime;

    public EtlWorker(
        ILogger<EtlWorker> logger,
        ExtractDataUseCase extractDataUseCase,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _extractDataUseCase = extractDataUseCase;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service ETL iniciado en: {Time}", DateTimeOffset.Now);

        try
        {
            // Ejecutar el proceso de extracción
            var result = await _extractDataUseCase.ExecuteAsync();

            // Mostrar resumen
            _logger.LogInformation("════════════════════════════════════════");
            _logger.LogInformation("       RESUMEN DE EXTRACCIÓN ETL        ");
            _logger.LogInformation("════════════════════════════════════════");
            _logger.LogInformation("✓ Clientes extraídos:  {Count}", result.CustomersExtracted);
            _logger.LogInformation("✓ Productos extraídos: {Count}", result.ProductsExtracted);
            _logger.LogInformation("✓ Órdenes extraídas:   {Count}", result.OrdersExtracted);
            _logger.LogInformation("⏱ Tiempo de ejecución: {Time}", result.ExecutionTime);
            _logger.LogInformation("════════════════════════════════════════");

            if (!result.Success)
            {
                _logger.LogError("El proceso ETL finalizó con errores: {Error}", result.ErrorMessage);
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