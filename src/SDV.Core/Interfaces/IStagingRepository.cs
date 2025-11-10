using SDV.Core.Entities.Staging;

namespace SDV.Core.Interfaces;

/// <summary>
/// Interfaz para operaciones con la capa staging
/// </summary>
public interface IStagingRepository
{
    Task BulkInsertCustomersAsync(IEnumerable<StagingCustomer> customers);
    Task BulkInsertProductsAsync(IEnumerable<StagingProduct> products);
    Task BulkInsertOrdersAsync(IEnumerable<StagingOrder> orders);
    Task BulkInsertOrderDetailsAsync(IEnumerable<StagingOrderDetail> orderDetails);
    Task<int> GetCustomerCountAsync();
    Task<int> GetProductCountAsync();
    Task<int> GetOrderCountAsync();
}