using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Dimensions;
using SDV.Core.Interfaces;

namespace SDV.Infrastructure.Loaders;

/// <summary>
/// Cargador de la dimensión DimProduct con soporte SCD Tipo 2
/// Lee datos desde StagingProducts y los carga en DimProduct
/// Rastrea cambios históricos especialmente en precios
/// </summary>
public class ProductDimensionLoader : IDimensionLoader
{
    private readonly IDimensionRepository _repository;
    private readonly ILogger<ProductDimensionLoader> _logger;

    public string LoaderName => "DimProduct Loader";

    public ProductDimensionLoader(
        IDimensionRepository repository, 
        ILogger<ProductDimensionLoader> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DimensionLoadResult> LoadAsync()
    {
        var result = new DimensionLoadResult
        {
            DimensionName = "DimProduct"
        };
        
        var startTime = DateTime.Now;

        try
        {
            _logger.LogInformation("Iniciando carga de DimProduct...");

            // Obtener productos del staging
            var stagingProducts = (await _repository.GetStagingProductsAsync()).ToList();
            result.RecordsProcessed = stagingProducts.Count;

            _logger.LogInformation("Procesando {Count} productos desde staging", stagingProducts.Count);

            var newProducts = new List<DimProduct>();

            foreach (var staging in stagingProducts)
            {
                // Buscar si existe registro actual en la dimensión
                var existingProduct = await _repository.GetCurrentProductAsync(staging.ProductID);

                if (existingProduct == null)
                {
                    // NUEVO: No existe en dimensión, insertar
                    var newProduct = new DimProduct
                    {
                        ProductID = staging.ProductID,
                        ProductName = staging.ProductName,
                        Category = staging.Category,
                        Price = staging.Price,
                        StartDate = DateTime.Today,
                        EndDate = new DateTime(9999, 12, 31),
                        IsCurrent = true
                    };
                    
                    newProducts.Add(newProduct);
                    result.RecordsInserted++;
                }
                else
                {
                    // EXISTENTE: Verificar si hay cambios (SCD Tipo 2)
                    if (HasChanges(existingProduct, staging))
                    {
                        // Cerrar registro anterior
                        existingProduct.EndDate = DateTime.Today.AddDays(-1);
                        existingProduct.IsCurrent = false;
                        await _repository.UpdateProductAsync(existingProduct);

                        // Crear nuevo registro con datos actualizados
                        var updatedProduct = new DimProduct
                        {
                            ProductID = staging.ProductID,
                            ProductName = staging.ProductName,
                            Category = staging.Category,
                            Price = staging.Price,
                            StartDate = DateTime.Today,
                            EndDate = new DateTime(9999, 12, 31),
                            IsCurrent = true
                        };
                        
                        newProducts.Add(updatedProduct);
                        result.RecordsUpdated++;
                        
                        _logger.LogDebug(
                            "Producto {ProductID} actualizado: Precio {OldPrice} → {NewPrice}",
                            staging.ProductID, existingProduct.Price, staging.Price);
                    }
                    else
                    {
                        // Sin cambios
                        result.RecordsUnchanged++;
                    }
                }
            }

            // Insertar nuevos registros en lote
            if (newProducts.Any())
            {
                await _repository.BulkInsertProductsAsync(newProducts);
            }

            await _repository.SaveChangesAsync();

            result.Success = true;
            result.ExecutionTime = DateTime.Now - startTime;

            _logger.LogInformation(
                "✓ DimProduct cargado: {Inserted} insertados, {Updated} actualizados, {Unchanged} sin cambios",
                result.RecordsInserted, result.RecordsUpdated, result.RecordsUnchanged);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTime.Now - startTime;
            
            _logger.LogError(ex, "❌ Error al cargar DimProduct");
            throw;
        }
    }

    /// <summary>
    /// Compara si hay cambios entre el registro actual y el staging
    /// El precio es especialmente importante para el historial
    /// </summary>
    private bool HasChanges(DimProduct existing, Core.Entities.Staging.StagingProduct staging)
    {
        return existing.ProductName != staging.ProductName ||
               existing.Category != staging.Category ||
               existing.Price != staging.Price;
    }
}
