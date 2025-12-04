namespace SDV.Core.Entities.Dimensions;

/// <summary>
/// Dimensión de estados de órdenes
/// Contiene valores predefinidos (Pending, Shipped, Delivered, Cancelled)
/// </summary>
public class DimStatus
{
    /// <summary>
    /// Clave surrogate (auto-incremental)
    /// </summary>
    public int StatusKey { get; set; }
    
    /// <summary>
    /// Nombre del estado (único)
    /// </summary>
    public string StatusName { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción del estado
    /// </summary>
    public string? StatusDescription { get; set; }
}
