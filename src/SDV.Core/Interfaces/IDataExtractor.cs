namespace SDV.Core.Interfaces;

/// <summary>
/// Interfaz genérica para extractores de datos (Strategy Pattern)
/// </summary>
/// <typeparam name="T">Tipo de entidad a extraer</typeparam>
public interface IDataExtractor<T> where T : class
{
    /// <summary>
    /// Extrae datos de la fuente
    /// </summary>
    /// <returns>Lista de entidades extraídas</returns>
    Task<IEnumerable<T>> ExtractAsync();
    
    /// Nombre identificador del extractor
    string ExtractorName { get; }
}