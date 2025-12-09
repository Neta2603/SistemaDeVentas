using SDV.Core.Entities.Facts;

namespace SDV.Core.Interfaces;

/// <summary>
/// Interfaz para el repositorio de tablas de hechos
/// </summary>
public interface IFactRepository
{
    /// <summary>
    /// Inserta un nuevo registro de hecho en el Data Warehouse
    /// </summary>
    Task AddFactAsync(FactSales fact);

    /// <summary>
    /// Inserta múltiples hechos en batch para mejor rendimiento
    /// </summary>
    Task AddRangeAsync(IEnumerable<FactSales> facts);

    /// <summary>
    /// Limpia (trunca) todas las tablas de hechos
    /// Similar a un "migrate:fresh" - borra todos los datos
    /// </summary>
    Task CleanupFactTablesAsync();

    /// <summary>
    /// Guarda todos los cambios en la base de datos
    /// </summary>
    Task SaveChangesAsync();
}
