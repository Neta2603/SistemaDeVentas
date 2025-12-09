using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SDV.Core.Entities.Facts;

/// <summary>
/// Tabla de hechos (Fact Table) del Data Warehouse
/// Grain: Una fila por cada línea de detalle de orden (producto x orden)
/// Métricas: Quantity, UnitPrice, TotalPrice
/// </summary>
[Table("FactSales")]
public class FactSales
{
    /// <summary>
    /// Clave primaria surrogate de la tabla de hechos
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SalesKey { get; set; }

    // ==================== CLAVES FORÁNEAS A DIMENSIONES ====================
    
    /// <summary>
    /// Clave foránea a DimCustomer
    /// </summary>
    [Required]
    public long CustomerKey { get; set; }

    /// <summary>
    /// Clave foránea a DimProduct
    /// </summary>
    [Required]
    public long ProductKey { get; set; }

    /// <summary>
    /// Clave foránea a DimTime
    /// </summary>
    [Required]
    public int TimeKey { get; set; }

    /// <summary>
    /// Clave foránea a DimStatus
    /// </summary>
    [Required]
    public int StatusKey { get; set; }

    // ==================== CLAVE DE NEGOCIO ====================
    
    /// <summary>
    /// ID de orden del sistema transaccional (clave de negocio)
    /// Permite trazabilidad con el sistema origen
    /// </summary>
    [Required]
    public int OrderID { get; set; }

    // ==================== MÉTRICAS (MEDIDAS) ====================
    
    /// <summary>
    /// Cantidad de unidades vendidas
    /// </summary>
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// Precio unitario en el momento de la venta
    /// Preserva el precio histórico del producto
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Precio total de la línea (Quantity * UnitPrice)
    /// Métrica calculada para análisis
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    // ==================== AUDITORÍA ====================
    
    /// <summary>
    /// Fecha y hora de carga en el Data Warehouse
    /// Para auditoría del proceso ETL
    /// </summary>
    public DateTime LoadDate { get; set; } = DateTime.Now;
}
