namespace SDV.Core.Entities.Dimensions;

/// <summary>
/// Dimensión de tiempo precalculada para análisis temporal
/// Esta tabla se genera mediante procedimiento almacenado en BD
/// </summary>
public class DimTime
{
    /// <summary>
    /// Clave primaria en formato YYYYMMDD (ej: 20250101)
    /// </summary>
    public int TimeKey { get; set; }
    
    public DateTime FullDate { get; set; }
    public int Year { get; set; }
    
    /// <summary>
    /// Trimestre (1-4)
    /// </summary>
    public int Quarter { get; set; }
    
    /// <summary>
    /// Mes (1-12)
    /// </summary>
    public int Month { get; set; }
    
    public string? MonthName { get; set; }
    
    /// <summary>
    /// Día del mes (1-31)
    /// </summary>
    public int Day { get; set; }
    
    /// <summary>
    /// Día de la semana (1=Domingo en MySQL)
    /// </summary>
    public int DayOfWeek { get; set; }
    
    public string? DayName { get; set; }
    public bool IsWeekend { get; set; }
    
    /// <summary>
    /// Flag para días festivos (opcional)
    /// </summary>
    public bool IsHoliday { get; set; }
}
