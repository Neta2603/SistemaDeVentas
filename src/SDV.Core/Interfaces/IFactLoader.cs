namespace SDV.Core.Interfaces;

/// <summary>
/// Interfaz para loaders de tablas de hechos (Fact Tables)
/// </summary>
public interface IFactLoader
{
    /// <summary>
    /// Carga los hechos desde las tablas staging al Data Warehouse
    /// </summary>
    /// <returns>Número de registros procesados</returns>
    Task<int> LoadAsync();
}
