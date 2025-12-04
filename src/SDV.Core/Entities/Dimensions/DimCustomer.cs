namespace SDV.Core.Entities.Dimensions;

/// <summary>
/// Dimensión de clientes con soporte para SCD Tipo 2
/// Permite rastrear cambios históricos en los datos del cliente
/// </summary>
public class DimCustomer
{
    /// <summary>
    /// Clave surrogate (auto-incremental)
    /// </summary>
    public long CustomerKey { get; set; }
    
    /// <summary>
    /// Clave de negocio (ID original del sistema transaccional)
    /// </summary>
    public int CustomerID { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    // Campos SCD Tipo 2
    /// <summary>
    /// Fecha de inicio de validez del registro
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Fecha de fin de validez (9999-12-31 para registros activos)
    /// </summary>
    public DateTime EndDate { get; set; } = new DateTime(9999, 12, 31);
    
    /// <summary>
    /// Indica si es la versión actual del registro
    /// </summary>
    public bool IsCurrent { get; set; } = true;
}
