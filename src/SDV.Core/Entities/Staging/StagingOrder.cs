namespace SDV.Core.Entities.Staging;

/// Entidad que representa la tabla staging de órdenes

public class StagingOrder
{
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
    public DateTime LoadDate { get; set; } = DateTime.Now;
}