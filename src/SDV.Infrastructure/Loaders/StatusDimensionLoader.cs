using Microsoft.Extensions.Logging;
using SDV.Core.Interfaces;

namespace SDV.Infrastructure.Loaders;

/// <summary>
/// Verificador/Cargador de DimStatus
/// DimStatus tiene valores predefinidos insertados en el script SQL
/// Este loader solo verifica que los datos existan
/// </summary>
public class StatusDimensionLoader : IDimensionLoader
{
    private readonly IDimensionRepository _repository;
    private readonly ILogger<StatusDimensionLoader> _logger;

    // Estados esperados según el script SQL
    private readonly string[] _expectedStatuses = { "Pending", "Shipped", "Delivered", "Cancelled" };

    public string LoaderName => "DimStatus Verifier";

    public StatusDimensionLoader(
        IDimensionRepository repository, 
        ILogger<StatusDimensionLoader> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DimensionLoadResult> LoadAsync()
    {
        var result = new DimensionLoadResult
        {
            DimensionName = "DimStatus"
        };
        
        var startTime = DateTime.Now;

        try
        {
            _logger.LogInformation("Verificando DimStatus...");

            // Obtener todos los estados
            var statuses = (await _repository.GetAllStatusesAsync()).ToList();
            var statusCount = statuses.Count;
            
            result.RecordsProcessed = statusCount;

            if (statusCount >= _expectedStatuses.Length)
            {
                // Verificar que todos los estados esperados existan
                var existingNames = statuses.Select(s => s.StatusName).ToHashSet();
                var missingStatuses = _expectedStatuses.Where(s => !existingNames.Contains(s)).ToList();

                if (!missingStatuses.Any())
                {
                    result.Success = true;
                    result.RecordsUnchanged = statusCount;
                    
                    _logger.LogInformation(
                        "✓ DimStatus verificado: {Count} estados disponibles ({Statuses})",
                        statusCount, string.Join(", ", statuses.Select(s => s.StatusName)));
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = $"Estados faltantes: {string.Join(", ", missingStatuses)}";
                    
                    _logger.LogWarning(
                        "⚠ DimStatus incompleto. Estados faltantes: {Missing}",
                        string.Join(", ", missingStatuses));
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = $"DimStatus tiene {statusCount} registros, se esperan {_expectedStatuses.Length}";
                
                _logger.LogError(
                    "❌ DimStatus incompleto ({Current}/{Expected}). Ejecute el script SQL inicial",
                    statusCount, _expectedStatuses.Length);
            }

            result.ExecutionTime = DateTime.Now - startTime;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTime.Now - startTime;
            
            _logger.LogError(ex, "❌ Error al verificar DimStatus");
            throw;
        }
    }
}
