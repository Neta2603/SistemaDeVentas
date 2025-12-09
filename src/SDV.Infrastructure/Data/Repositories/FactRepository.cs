using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Facts;
using SDV.Core.Interfaces;
using SDV.Infrastructure.Data;

namespace SDV.Infrastructure.Data.Repositories;

/// <summary>
/// Implementación del repositorio para tablas de hechos (Fact Tables)
/// Maneja la persistencia de FactSales en el Data Warehouse
/// </summary>
public class FactRepository : IFactRepository
{
    private readonly StagingDbContext _context;
    private readonly ILogger<FactRepository> _logger;

    public FactRepository(StagingDbContext context, ILogger<FactRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Inserta un nuevo registro de hecho
    /// </summary>
    public async Task AddFactAsync(FactSales fact)
    {
        await _context.FactSales.AddAsync(fact);
    }

    /// <summary>
    /// Inserta múltiples hechos en batch para mejor rendimiento
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<FactSales> facts)
    {
        await _context.FactSales.AddRangeAsync(facts);
    }

    /// <summary>
    /// Limpia (trunca) la tabla de hechos FactSales
    /// Similar a migrate:fresh - borra TODOS los datos de la tabla
    /// </summary>
    public async Task CleanupFactTablesAsync()
    {
        _logger.LogInformation("Iniciando limpieza de tabla de hechos FactSales...");

        try
        {
            // Ejecutar TRUNCATE TABLE para limpiar completamente la tabla
            // TRUNCATE es más eficiente que DELETE porque:
            // 1. No genera logs de transacción por cada fila
            // 2. Reinicia los contadores de identidad
            // 3. Es instantáneo independientemente del tamaño de la tabla
            await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 0");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE FactSales");
            await _context.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 1");

            _logger.LogInformation("✓ Tabla FactSales limpiada exitosamente (TRUNCATE)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar la tabla FactSales");
            throw;
        }
    }

    /// <summary>
    /// Guarda todos los cambios en la base de datos
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
