namespace SDV.Core.Entities.Staging;

/// Entidad que representa la tabla staging de clientes
public class StagingCustomer
{
    public int CustomerID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public DateTime LoadDate { get; set; } = DateTime.Now;
}