using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Facts;
using SDV.Core.Interfaces;
using SDV.Infrastructure.Data;

namespace SDV.Infrastructure.Loaders;

/// <summary>
/// Loader para cargar la tabla de hechos FactSales desde staging
/// Realiza la transformación y carga de los hechos de ventas
/// OPTIMIZADO: Carga todo en memoria y hace JOINs rápidos
/// </summary>
public class FactSalesLoader : IFactLoader
{
    private readonly StagingDbContext _context;
    private readonly IFactRepository _factRepository;
    private readonly ILogger<FactSalesLoader> _logger;

    public FactSalesLoader(
        StagingDbContext context,
        IFactRepository factRepository,
        ILogger<FactSalesLoader> logger)
    {
        _context = context;
        _factRepository = factRepository;
        _logger = logger;
    }

    /// <summary>
    /// Carga los hechos de ventas desde staging al Data Warehouse
    /// OPTIMIZADO: Carga todas las dimensiones en memoria primero
    /// </summary>
    public async Task<int> LoadAsync()
    {
        _logger.LogInformation("Iniciando carga de FactSales...");
        var startTime = DateTime.Now;

        try
        {
            // ==================== PASO 1: CARGAR DIMENSIONES EN MEMORIA ====================
            _logger.LogInformation("Cargando dimensiones en memoria...");

            var customers = await _context.DimCustomers
                .AsNoTracking()
                .Where(c => c.IsCurrent)
                .ToDictionaryAsync(c => c.CustomerID, c => c.CustomerKey);

            var products = await _context.DimProducts
                .AsNoTracking()
                .Where(p => p.IsCurrent)
                .ToDictionaryAsync(p => p.ProductID, p => new { p.ProductKey, p.Price });

            var times = await _context.DimTimes
                .AsNoTracking()
                .ToDictionaryAsync(t => t.FullDate, t => t.TimeKey);

            var statuses = await _context.DimStatuses
                .AsNoTracking()
                .ToDictionaryAsync(s => s.StatusName, s => s.StatusKey);

            _logger.LogInformation("✓ Dimensiones cargadas: {Customers} customers, {Products} products, {Times} times, {Statuses} statuses",
                customers.Count, products.Count, times.Count, statuses.Count);

            // ==================== PASO 2: CARGAR STAGING EN MEMORIA ====================
            _logger.LogInformation("Cargando datos de staging...");

            var stagingOrders = await _context.StagingOrders
                .AsNoTracking()
                .GroupBy(o => o.OrderID)
                .Select(g => new { 
                    OrderID = g.Key, 
                    CustomerID = g.First().CustomerID, 
                    OrderDate = g.First().OrderDate, 
                    Status = g.First().Status 
                })
                .ToDictionaryAsync(o => o.OrderID);

            var stagingOrderDetails = await _context.StagingOrderDetails
                .AsNoTracking()
                .ToListAsync();

            if (!stagingOrderDetails.Any())
            {
                _logger.LogWarning("No hay detalles de órdenes en staging para procesar");
                return 0;
            }

            _logger.LogInformation("✓ Staging cargado: {Orders} orders, {Details} details",
                stagingOrders.Count, stagingOrderDetails.Count);

            // ==================== PASO 3: TRANSFORMAR Y CARGAR ====================
            _logger.LogInformation("Transformando y cargando FactSales...");

            var factsToInsert = new List<FactSales>();
            int processed = 0;
            int skipped = 0;
            int skippedNoOrder = 0;
            int skippedNoCustomer = 0;
            int skippedNoProduct = 0;
            int skippedNoTime = 0;
            int skippedNoStatus = 0;

            foreach (var detail in stagingOrderDetails)
            {
                processed++;

                // Buscar orden
                if (!stagingOrders.TryGetValue(detail.OrderID, out var order))
                {
                    skippedNoOrder++;
                    skipped++;
                    continue;
                }

                // Buscar CustomerKey
                if (!customers.TryGetValue(order.CustomerID, out var customerKey))
                {
                    skippedNoCustomer++;
                    skipped++;
                    continue;
                }

                // Buscar ProductKey y Price
                if (!products.TryGetValue(detail.ProductID, out var product))
                {
                    skippedNoProduct++;
                    skipped++;
                    continue;
                }

                // Buscar TimeKey
                if (!times.TryGetValue(order.OrderDate, out var timeKey))
                {
                    skippedNoTime++;
                    skipped++;
                    continue;
                }

                // Buscar StatusKey
                if (string.IsNullOrEmpty(order.Status) || !statuses.TryGetValue(order.Status, out var statusKey))
                {
                    skippedNoStatus++;
                    skipped++;
                    continue;
                }

                // Calcular métricas
                var unitPrice = product.Price;
                var totalPrice = detail.Quantity * unitPrice;

                // Crear el registro de FactSales
                var fact = new FactSales
                {
                    CustomerKey = customerKey,
                    ProductKey = product.ProductKey,
                    TimeKey = timeKey,
                    StatusKey = statusKey,
                    OrderID = order.OrderID,
                    Quantity = detail.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    LoadDate = DateTime.Now
                };

                factsToInsert.Add(fact);

                // Log progreso cada 50 registros
                if (processed % 50 == 0)
                {
                    _logger.LogInformation("Procesando... {Processed}/{Total} ({Percent}%)", 
                        processed, stagingOrderDetails.Count, 
                        (processed * 100) / stagingOrderDetails.Count);
                }
            }

            // ==================== PASO 4: INSERTAR EN BATCH ====================
            if (factsToInsert.Any())
            {
                _logger.LogInformation("Insertando {Count} registros en FactSales...", factsToInsert.Count);
                await _factRepository.AddRangeAsync(factsToInsert);
                await _factRepository.SaveChangesAsync();

                var duration = DateTime.Now - startTime;
                _logger.LogInformation("✓ FactSales cargada exitosamente");
                _logger.LogInformation("  • Total procesados: {Processed}", processed);
                _logger.LogInformation("  • Insertados: {Inserted}", factsToInsert.Count);
                _logger.LogInformation("  • Omitidos: {Skipped}", skipped);
                if (skipped > 0)
                {
                    _logger.LogWarning("  DESGLOSE DE OMITIDOS:");
                    if (skippedNoOrder > 0) _logger.LogWarning("    - Sin orden en staging: {Count}", skippedNoOrder);
                    if (skippedNoCustomer > 0) _logger.LogWarning("    - Sin cliente en DimCustomer: {Count}", skippedNoCustomer);
                    if (skippedNoProduct > 0) _logger.LogWarning("    - Sin producto en DimProduct: {Count}", skippedNoProduct);
                    if (skippedNoTime > 0) _logger.LogWarning("    - Sin fecha en DimTime: {Count}", skippedNoTime);
                    if (skippedNoStatus > 0) _logger.LogWarning("    - Sin status en DimStatus: {Count}", skippedNoStatus);
                }
                _logger.LogInformation("  • Tiempo: {Duration}", duration);
            }
            else
            {
                _logger.LogWarning("No se generaron registros para insertar en FactSales");
                _logger.LogWarning("Total omitidos: {Skipped}", skipped);
            }

            return factsToInsert.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cargando FactSales");
            throw;
        }
    }
}
