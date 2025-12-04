using Microsoft.Extensions.Logging;
using SDV.Core.Entities.Dimensions;
using SDV.Core.Interfaces;

namespace SDV.Infrastructure.Loaders;

/// <summary>
/// Cargador de la dimensión DimCustomer con soporte SCD Tipo 2
/// Lee datos desde StagingCustomers y los carga en DimCustomer
/// </summary>
public class CustomerDimensionLoader : IDimensionLoader
{
    private readonly IDimensionRepository _repository;
    private readonly ILogger<CustomerDimensionLoader> _logger;

    public string LoaderName => "DimCustomer Loader";

    public CustomerDimensionLoader(
        IDimensionRepository repository, 
        ILogger<CustomerDimensionLoader> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DimensionLoadResult> LoadAsync()
    {
        var result = new DimensionLoadResult
        {
            DimensionName = "DimCustomer"
        };
        
        var startTime = DateTime.Now;

        try
        {
            _logger.LogInformation("Iniciando carga de DimCustomer...");

            // Obtener clientes del staging
            var stagingCustomers = (await _repository.GetStagingCustomersAsync()).ToList();
            result.RecordsProcessed = stagingCustomers.Count;

            _logger.LogInformation("Procesando {Count} clientes desde staging", stagingCustomers.Count);

            var newCustomers = new List<DimCustomer>();

            foreach (var staging in stagingCustomers)
            {
                // Buscar si existe registro actual en la dimensión
                var existingCustomer = await _repository.GetCurrentCustomerAsync(staging.CustomerID);

                if (existingCustomer == null)
                {
                    // NUEVO: No existe en dimensión, insertar
                    var newCustomer = new DimCustomer
                    {
                        CustomerID = staging.CustomerID,
                        FirstName = staging.FirstName,
                        LastName = staging.LastName,
                        Email = staging.Email,
                        Phone = staging.Phone,
                        City = staging.City,
                        Country = staging.Country,
                        StartDate = DateTime.Today,
                        EndDate = new DateTime(9999, 12, 31),
                        IsCurrent = true
                    };
                    
                    newCustomers.Add(newCustomer);
                    result.RecordsInserted++;
                }
                else
                {
                    // EXISTENTE: Verificar si hay cambios (SCD Tipo 2)
                    if (HasChanges(existingCustomer, staging))
                    {
                        // Cerrar registro anterior
                        existingCustomer.EndDate = DateTime.Today.AddDays(-1);
                        existingCustomer.IsCurrent = false;
                        await _repository.UpdateCustomerAsync(existingCustomer);

                        // Crear nuevo registro con datos actualizados
                        var updatedCustomer = new DimCustomer
                        {
                            CustomerID = staging.CustomerID,
                            FirstName = staging.FirstName,
                            LastName = staging.LastName,
                            Email = staging.Email,
                            Phone = staging.Phone,
                            City = staging.City,
                            Country = staging.Country,
                            StartDate = DateTime.Today,
                            EndDate = new DateTime(9999, 12, 31),
                            IsCurrent = true
                        };
                        
                        newCustomers.Add(updatedCustomer);
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        // Sin cambios
                        result.RecordsUnchanged++;
                    }
                }
            }

            // Insertar nuevos registros en lote
            if (newCustomers.Any())
            {
                await _repository.BulkInsertCustomersAsync(newCustomers);
            }

            await _repository.SaveChangesAsync();

            result.Success = true;
            result.ExecutionTime = DateTime.Now - startTime;

            _logger.LogInformation(
                "✓ DimCustomer cargado: {Inserted} insertados, {Updated} actualizados, {Unchanged} sin cambios",
                result.RecordsInserted, result.RecordsUpdated, result.RecordsUnchanged);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = DateTime.Now - startTime;
            
            _logger.LogError(ex, "❌ Error al cargar DimCustomer");
            throw;
        }
    }

    /// <summary>
    /// Compara si hay cambios entre el registro actual y el staging
    /// </summary>
    private bool HasChanges(DimCustomer existing, Core.Entities.Staging.StagingCustomer staging)
    {
        return existing.FirstName != staging.FirstName ||
               existing.LastName != staging.LastName ||
               existing.Email != staging.Email ||
               existing.Phone != staging.Phone ||
               existing.City != staging.City ||
               existing.Country != staging.Country;
    }
}
