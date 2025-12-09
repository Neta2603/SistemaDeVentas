using Microsoft.Extensions.Logging;
using SDV.Core.Interfaces;

namespace SDV.Application.UseCases;

/// <summary>
/// Caso de uso para cargar tablas de hechos (Fact Tables) en el Data Warehouse
/// Fase 3 del proceso ETL: Transform & Load Facts
/// </summary>
public class LoadFactsUseCase
{
    private readonly IFactRepository _factRepository;
    private readonly IFactLoader _factSalesLoader;
    private readonly ILogger<LoadFactsUseCase> _logger;

    public LoadFactsUseCase(
        IFactRepository factRepository,
        IFactLoader factSalesLoader,
        ILogger<LoadFactsUseCase> logger)
    {
        _factRepository = factRepository;
        _factSalesLoader = factSalesLoader;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta la carga de todas las tablas de hechos
    /// 1. Limpia (trunca) las tablas de hechos existentes
    /// 2. Carga FactSales desde staging
    /// </summary>
    public async Task<LoadFactsResult> ExecuteAsync()
    {
        var startTime = DateTime.Now;
        _logger.LogInformation("════════════════════════════════════════");
        _logger.LogInformation("  INICIANDO CARGA DE TABLAS DE HECHOS  ");
        _logger.LogInformation("════════════════════════════════════════");

        try
        {
            // ==================== PASO 1: LIMPIEZA ====================
            _logger.LogInformation("");
            _logger.LogInformation("┌────────────────────────────────────────┐");
            _logger.LogInformation("│ PASO 1: Limpieza de Tablas de Hechos  │");
            _logger.LogInformation("└────────────────────────────────────────┘");
            
            await _factRepository.CleanupFactTablesAsync();
            
            _logger.LogInformation("✓ Limpieza completada");

            // ==================== PASO 2: CARGA DE FACTSALES ====================
            _logger.LogInformation("");
            _logger.LogInformation("┌────────────────────────────────────────┐");
            _logger.LogInformation("│ PASO 2: Carga de FactSales             │");
            _logger.LogInformation("└────────────────────────────────────────┘");
            
            var factSalesCount = await _factSalesLoader.LoadAsync();

            // ==================== RESUMEN ====================
            var duration = DateTime.Now - startTime;

            _logger.LogInformation("");
            _logger.LogInformation("════════════════════════════════════════");
            _logger.LogInformation("     CARGA DE HECHOS COMPLETADA         ");
            _logger.LogInformation("════════════════════════════════════════");
            _logger.LogInformation("• FactSales insertados: {Count}", factSalesCount);
            _logger.LogInformation("• Tiempo total: {Duration}", duration);
            _logger.LogInformation("════════════════════════════════════════");

            return new LoadFactsResult
            {
                Success = true,
                FactSalesLoaded = factSalesCount,
                ExecutionTime = duration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico durante la carga de hechos");
            
            return new LoadFactsResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.Now - startTime
            };
        }
    }
}

/// <summary>
/// Resultado de la carga de tablas de hechos
/// </summary>
public class LoadFactsResult
{
    public bool Success { get; set; }
    public int FactSalesLoaded { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}
