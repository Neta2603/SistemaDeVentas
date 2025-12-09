using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;

namespace SDV.Infrastructure.Data.Repositories;

/// <summary>
/// Implementación del repositorio para operaciones con staging
/// </summary>
public class StagingRepository : IStagingRepository
{
    private readonly StagingDbContext _context;
    private readonly ILogger<StagingRepository> _logger;

    public StagingRepository(StagingDbContext context, ILogger<StagingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task BulkInsertCustomersAsync(IEnumerable<StagingCustomer> customers)
    {
        try
        {
            await _context.StagingCustomers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Insertados {Count} clientes en staging", customers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error insertando clientes en staging");
            throw;
        }
    }

    public async Task BulkInsertProductsAsync(IEnumerable<StagingProduct> products)
    {
        try
        {
            await _context.StagingProducts.AddRangeAsync(products);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Insertados {Count} productos en staging", products.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error insertando productos en staging");
            throw;
        }
    }

    public async Task BulkInsertOrdersAsync(IEnumerable<StagingOrder> orders)
    {
        try
        {
            await _context.StagingOrders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Insertadas {Count} órdenes en staging", orders.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error insertando órdenes en staging");
            throw;
        }
    }

    public async Task BulkInsertOrderDetailsAsync(IEnumerable<StagingOrderDetail> orderDetails)
    {
        try
        {
            await _context.StagingOrderDetails.AddRangeAsync(orderDetails);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Insertados {Count} detalles de órdenes en staging", orderDetails.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error insertando detalles de órdenes en staging");
            throw;
        }
    }

    public async Task<int> GetCustomerCountAsync()
    {
        return await _context.StagingCustomers.CountAsync();
    }

    public async Task<int> GetProductCountAsync()
    {
        return await _context.StagingProducts.CountAsync();
    }

    public async Task<int> GetOrderCountAsync()
    {
        return await _context.StagingOrders.CountAsync();
    }

    /// <summary>
    /// Limpia todas las tablas staging (TRUNCATE)
    /// Similar a "migrate:fresh" - elimina todos los datos para empezar limpio
    /// </summary>
    public async Task CleanupStagingTablesAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando limpieza de tablas staging...");

            // Deshabilitar validación de foreign keys temporalmente
            await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 0");

            // Limpiar cada tabla staging
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE StagingCustomers");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE StagingProducts");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE StagingOrders");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE StagingOrderDetails");

            // Rehabilitar validación de foreign keys
            await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 1");

            _logger.LogInformation("✓ Todas las tablas staging limpiadas exitosamente (TRUNCATE)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error limpiando tablas staging");
            throw;
        }
    }
}