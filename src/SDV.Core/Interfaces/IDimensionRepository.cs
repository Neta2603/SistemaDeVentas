using SDV.Core.Entities.Dimensions;
using SDV.Core.Entities.Staging;

namespace SDV.Core.Interfaces;

/// <summary>
/// Interfaz para operaciones con tablas de dimensiones del Data Warehouse
/// </summary>
public interface IDimensionRepository
{
    // ==================== DimCustomer ====================
    /// <summary>
    /// Obtiene el cliente actual por CustomerID (IsCurrent = true)
    /// </summary>
    Task<DimCustomer?> GetCurrentCustomerAsync(int customerId);
    
    /// <summary>
    /// Inserta un nuevo registro en DimCustomer
    /// </summary>
    Task InsertCustomerAsync(DimCustomer customer);
    
    /// <summary>
    /// Actualiza un registro existente (para cerrar versión SCD2)
    /// </summary>
    Task UpdateCustomerAsync(DimCustomer customer);
    
    /// <summary>
    /// Inserta múltiples clientes en lote
    /// </summary>
    Task BulkInsertCustomersAsync(IEnumerable<DimCustomer> customers);
    
    /// <summary>
    /// Obtiene el conteo de clientes actuales
    /// </summary>
    Task<int> GetCustomerCountAsync();

    // ==================== DimProduct ====================
    /// <summary>
    /// Obtiene el producto actual por ProductID (IsCurrent = true)
    /// </summary>
    Task<DimProduct?> GetCurrentProductAsync(int productId);
    
    /// <summary>
    /// Inserta un nuevo registro en DimProduct
    /// </summary>
    Task InsertProductAsync(DimProduct product);
    
    /// <summary>
    /// Actualiza un registro existente (para cerrar versión SCD2)
    /// </summary>
    Task UpdateProductAsync(DimProduct product);
    
    /// <summary>
    /// Inserta múltiples productos en lote
    /// </summary>
    Task BulkInsertProductsAsync(IEnumerable<DimProduct> products);
    
    /// <summary>
    /// Obtiene el conteo de productos actuales
    /// </summary>
    Task<int> GetProductCountAsync();

    // ==================== DimTime ====================
    /// <summary>
    /// Verifica si DimTime tiene datos
    /// </summary>
    Task<int> GetTimeRecordCountAsync();
    
    /// <summary>
    /// Obtiene un registro de tiempo por fecha
    /// </summary>
    Task<DimTime?> GetTimeByDateAsync(DateTime date);

    // ==================== DimStatus ====================
    /// <summary>
    /// Obtiene todos los estados
    /// </summary>
    Task<IEnumerable<DimStatus>> GetAllStatusesAsync();
    
    /// <summary>
    /// Verifica si DimStatus tiene datos
    /// </summary>
    Task<int> GetStatusCountAsync();
    
    /// <summary>
    /// Obtiene un estado por nombre
    /// </summary>
    Task<DimStatus?> GetStatusByNameAsync(string statusName);

    // ==================== Staging Reads ====================
    /// <summary>
    /// Obtiene todos los clientes del staging para cargar a dimensión
    /// </summary>
    Task<IEnumerable<StagingCustomer>> GetStagingCustomersAsync();
    
    /// <summary>
    /// Obtiene todos los productos del staging para cargar a dimensión
    /// </summary>
    Task<IEnumerable<StagingProduct>> GetStagingProductsAsync();

    // ==================== Transaction ====================
    /// <summary>
    /// Guarda todos los cambios pendientes
    /// </summary>
    Task SaveChangesAsync();
}
