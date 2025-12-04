using Microsoft.Extensions.Logging;
using SDV.Core.Interfaces;

namespace SDV.Infrastructure.Loaders;

/// <summary>
/// Verificador/Cargador de DimTime
/// DimTime se genera mediante procedimiento almacenado en la BD
/// Este loader solo verifica que los datos existan
/// </summary>
public class TimeDimensionLoader : IDimensionLoader
{
    private readonly IDimensionRepository _repository;
    private readonly ILogger<TimeDimensionLoader> _logger;

    public string LoaderName => "DimTime Verifier";

    public TimeDimensionLoader(
        IDimensionRepository repository, 
        ILogger<TimeDimensionLoader> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DimensionLoadResult> LoadAsync()
    {
        var result = new DimensionLoadResult
        {
            DimensionName = "DimTime"
        };
        
        var startTime = DateTime.Now;

        try
        {
            _logger.LogInformation("Verificando DimTime...");

            // Verificar que DimTime tiene datos
            var recordCount = await _repository.GetTimeRecordCountAsync();
            
            result.RecordsProcessed = recordCount;

            if (recordCount > 0)
            {
                result.Success = true;
                result.RecordsUnchanged = recordCount;
                
                // Verificar rango de fechas
                var today = DateTime.Today;
                var todayRecord = await _repository.GetTimeByDateAsync(today);
                
                if (todayRecord != null)
                {
                    _logger.LogInformation(
                        "✓ DimTime verificado: {Count} registros, fecha actual disponible ({Year}-{Month}-{Day})",
                        recordCount, todayRecord.Year, todayRecord.Month, todayRecord.Day);
                }
                else
                {
                    _logger.LogWarning(
                        "⚠ DimTime tiene {Count} registros pero la fecha actual no está disponible",
                        recordCount);
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "DimTime está vacío. Ejecutar: CALL PopulateDimTime(2020, 2030);";
                
                _logger.LogError(
                    "❌ DimTime está vacío. Ejecute el procedimiento PopulateDimTime en la BD");
            }

            result.ExecutionTime = DateTime.Now - startTime;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTime.Now - startTime;
            
            _logger.LogError(ex, "❌ Error al verificar DimTime");
            throw;
        }
    }
}
