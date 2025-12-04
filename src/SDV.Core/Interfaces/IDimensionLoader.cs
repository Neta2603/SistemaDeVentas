namespace SDV.Core.Interfaces;

/// <summary>
/// Interfaz genérica para cargadores de dimensiones (Strategy Pattern)
/// Similar a IDataExtractor pero para la fase de carga (Load)
/// </summary>
public interface IDimensionLoader
{
    /// <summary>
    /// Nombre identificador del cargador
    /// </summary>
    string LoaderName { get; }
    
    /// <summary>
    /// Ejecuta la carga de la dimensión
    /// </summary>
    /// <returns>Resultado con estadísticas de la carga</returns>
    Task<DimensionLoadResult> LoadAsync();
}

/// <summary>
/// Resultado de la carga de una dimensión
/// </summary>
public class DimensionLoadResult
{
    /// <summary>
    /// Nombre de la dimensión cargada
    /// </summary>
    public string DimensionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Total de registros procesados desde staging
    /// </summary>
    public int RecordsProcessed { get; set; }
    
    /// <summary>
    /// Registros nuevos insertados
    /// </summary>
    public int RecordsInserted { get; set; }
    
    /// <summary>
    /// Registros actualizados (SCD Tipo 2 - nueva versión)
    /// </summary>
    public int RecordsUpdated { get; set; }
    
    /// <summary>
    /// Registros sin cambios (ignorados)
    /// </summary>
    public int RecordsUnchanged { get; set; }
    
    /// <summary>
    /// Indica si la carga fue exitosa
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Mensaje de error si la carga falló
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Tiempo de ejecución de la carga
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }
}
