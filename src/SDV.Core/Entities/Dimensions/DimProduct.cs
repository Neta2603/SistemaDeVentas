namespace SDV.Core.Entities.Dimensions;

/// <summary>
/// Dimensión de productos con soporte para SCD Tipo 2
/// Permite rastrear cambios históricos en precios y atributos
/// </summary>
public class DimProduct
{
    /// <summary>
    /// Clave surrogate (auto-incremental)
    /// </summary>
    public long ProductKey { get; set; }
    
    /// <summary>
    /// Clave de negocio (ID original del sistema transaccional)
    /// </summary>
    public int ProductID { get; set; }
    
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    
    /// <summary>
    /// Precio histórico del producto
    /// </summary>
    public decimal Price { get; set; }
    
    // Campos SCD Tipo 2
    /// <summary>
    /// Fecha de inicio de validez del precio
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Fecha de fin de validez (9999-12-31 para registros activos)
    /// </summary>
    public DateTime EndDate { get; set; } = new DateTime(9999, 12, 31);
    
    /// <summary>
    /// Indica si es el precio actual
    /// </summary>
    public bool IsCurrent { get; set; } = true;
}
