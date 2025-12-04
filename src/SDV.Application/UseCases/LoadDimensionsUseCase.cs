using Microsoft.Extensions.Logging;
using SDV.Core.Interfaces;

namespace SDV.Application.UseCases;

/// <summary>
/// Caso de uso para orquestar la carga de dimensiones del Data Warehouse
/// Ejecuta los loaders en el orden correcto
/// </summary>
public class LoadDimensionsUseCase
{
    private readonly IEnumerable<IDimensionLoader> _dimensionLoaders;
    private readonly ILogger<LoadDimensionsUseCase> _logger;

    public LoadDimensionsUseCase(
        IEnumerable<IDimensionLoader> dimensionLoaders,
        ILogger<LoadDimensionsUseCase> logger)
    {
        _dimensionLoaders = dimensionLoaders;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el proceso completo de carga de dimensiones
    /// </summary>
    public async Task<DimensionLoadSummary> ExecuteAsync()
    {
        var summary = new DimensionLoadSummary();
        var startTime = DateTime.Now;

        _logger.LogInformation("═══════════════════════════════════════════════════════");
        _logger.LogInformation("      INICIANDO CARGA DE DIMENSIONES DEL DW");
        _logger.LogInformation("═══════════════════════════════════════════════════════");

        try
        {
            foreach (var loader in _dimensionLoaders)
            {
                _logger.LogInformation("────────────────────────────────────────");
                _logger.LogInformation("Ejecutando: {LoaderName}", loader.LoaderName);
                
                var result = await loader.LoadAsync();
                summary.Results.Add(result);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "⚠ {DimensionName} completado con advertencias: {Error}",
                        result.DimensionName, result.ErrorMessage);
                }
            }

            summary.Success = summary.Results.All(r => r.Success);
            summary.ExecutionTime = DateTime.Now - startTime;

            _logger.LogInformation("═══════════════════════════════════════════════════════");
            _logger.LogInformation("      RESUMEN DE CARGA DE DIMENSIONES");
            _logger.LogInformation("═══════════════════════════════════════════════════════");
            
            foreach (var result in summary.Results)
            {
                var status = result.Success ? "✓" : "⚠";
                _logger.LogInformation(
                    "{Status} {Dimension}: {Processed} procesados | {Inserted} insertados | {Updated} actualizados | {Unchanged} sin cambios",
                    status, result.DimensionName, result.RecordsProcessed,
                    result.RecordsInserted, result.RecordsUpdated, result.RecordsUnchanged);
            }
            
            _logger.LogInformation("────────────────────────────────────────");
            _logger.LogInformation("⏱ Tiempo total de ejecución: {Time}", summary.ExecutionTime);
            _logger.LogInformation("═══════════════════════════════════════════════════════");

            return summary;
        }
        catch (Exception ex)
        {
            summary.Success = false;
            summary.ErrorMessage = ex.Message;
            summary.ExecutionTime = DateTime.Now - startTime;
            
            _logger.LogError(ex, "❌ Error durante la carga de dimensiones");
            throw;
        }
    }
}

/// <summary>
/// Resumen del proceso de carga de dimensiones
/// </summary>
public class DimensionLoadSummary
{
    public bool Success { get; set; }
    public List<DimensionLoadResult> Results { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
    public string? ErrorMessage { get; set; }

    public int TotalRecordsProcessed => Results.Sum(r => r.RecordsProcessed);
    public int TotalRecordsInserted => Results.Sum(r => r.RecordsInserted);
    public int TotalRecordsUpdated => Results.Sum(r => r.RecordsUpdated);
    public int TotalRecordsUnchanged => Results.Sum(r => r.RecordsUnchanged);
}
