namespace SDV.Core.Entities.Staging;

/// Entidad que representa la tabla staging de detalles de órdenes

public class StagingOrderDetail
{
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime LoadDate { get; set; } = DateTime.Now;
}