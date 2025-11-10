namespace SDV.Core.Entities.Staging;

/// Entidad que representa la tabla staging de productos
public class StagingProduct
{
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime LoadDate { get; set; } = DateTime.Now;
}