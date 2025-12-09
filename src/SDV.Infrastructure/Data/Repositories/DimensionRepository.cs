using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Dimensions;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;
using SDV.Infrastructure.Data;

namespace SDV.Infrastructure.Data.Repositories;

/// <summary>
/// Implementación del repositorio para tablas de dimensiones
/// </summary>
public class DimensionRepository : IDimensionRepository
{
    private readonly StagingDbContext _context;
    private readonly ILogger<DimensionRepository> _logger;

    public DimensionRepository(StagingDbContext context, ILogger<DimensionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ==================== DimCustomer ====================

    public async Task<DimCustomer?> GetCurrentCustomerAsync(int customerId)
    {
        return await _context.DimCustomers
            .FirstOrDefaultAsync(c => c.CustomerID == customerId && c.IsCurrent);
    }

    public async Task InsertCustomerAsync(DimCustomer customer)
    {
        await _context.DimCustomers.AddAsync(customer);
    }

    public async Task UpdateCustomerAsync(DimCustomer customer)
    {
        _context.DimCustomers.Update(customer);
        await Task.CompletedTask;
    }

    public async Task BulkInsertCustomersAsync(IEnumerable<DimCustomer> customers)
    {
        var customerList = customers.ToList();
        if (!customerList.Any()) return;

        await _context.DimCustomers.AddRangeAsync(customerList);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Insertados {Count} clientes en DimCustomer", customerList.Count);
    }

    public async Task<int> GetCustomerCountAsync()
    {
        return await _context.DimCustomers.CountAsync(c => c.IsCurrent);
    }

    // ==================== DimProduct ====================

    public async Task<DimProduct?> GetCurrentProductAsync(int productId)
    {
        return await _context.DimProducts
            .FirstOrDefaultAsync(p => p.ProductID == productId && p.IsCurrent);
    }

    public async Task InsertProductAsync(DimProduct product)
    {
        await _context.DimProducts.AddAsync(product);
    }

    public async Task UpdateProductAsync(DimProduct product)
    {
        _context.DimProducts.Update(product);
        await Task.CompletedTask;
    }

    public async Task BulkInsertProductsAsync(IEnumerable<DimProduct> products)
    {
        var productList = products.ToList();
        if (!productList.Any()) return;

        await _context.DimProducts.AddRangeAsync(productList);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Insertados {Count} productos en DimProduct", productList.Count);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await _context.DimProducts.CountAsync(p => p.IsCurrent);
    }

    // ==================== DimTime ====================

    public async Task<int> GetTimeRecordCountAsync()
    {
        return await _context.DimTimes.CountAsync();
    }

    public async Task<DimTime?> GetTimeByDateAsync(DateTime date)
    {
        var timeKey = int.Parse(date.ToString("yyyyMMdd"));
        return await _context.DimTimes.FindAsync(timeKey);
    }

    // ==================== DimStatus ====================

    public async Task<IEnumerable<DimStatus>> GetAllStatusesAsync()
    {
        return await _context.DimStatuses.ToListAsync();
    }

    public async Task<int> GetStatusCountAsync()
    {
        return await _context.DimStatuses.CountAsync();
    }

    public async Task<DimStatus?> GetStatusByNameAsync(string statusName)
    {
        return await _context.DimStatuses
            .FirstOrDefaultAsync(s => s.StatusName == statusName);
    }

    // ==================== Staging Reads ====================

    public async Task<IEnumerable<StagingCustomer>> GetStagingCustomersAsync()
    {
        // Obtener el registro más reciente de cada CustomerID
        return await _context.StagingCustomers
            .GroupBy(c => c.CustomerID)
            .Select(g => g.OrderByDescending(c => c.LoadDate).First())
            .ToListAsync();
    }

    public async Task<IEnumerable<StagingProduct>> GetStagingProductsAsync()
    {
        // Obtener el registro más reciente de cada ProductID
        return await _context.StagingProducts
            .GroupBy(p => p.ProductID)
            .Select(g => g.OrderByDescending(p => p.LoadDate).First())
            .ToListAsync();
    }

    // ==================== Transaction ====================

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
