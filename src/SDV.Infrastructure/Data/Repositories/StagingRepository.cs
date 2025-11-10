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
}