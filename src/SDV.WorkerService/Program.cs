using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SDV.Application.UseCases;
using SDV.Core.Entities.Staging;
using SDV.Core.Interfaces;
using SDV.Infrastructure.Data;
using SDV.Infrastructure.Data.Repositories;
using SDV.Infrastructure.Extractors.Api;
using SDV.Infrastructure.Extractors.Csv;
using SDV.Infrastructure.Extractors.Database;
using SDV.WorkerService;

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/etl-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("═══════════════════════════════════════════════════════");
    Log.Information("   SISTEMA DE ANÁLISIS DE VENTAS - ETL EXTRACTION    ");
    Log.Information("═══════════════════════════════════════════════════════");

    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;

            // Configuración de DbContext con MySQL
            var connectionString = configuration.GetConnectionString("StagingDb") 
                ?? "Server=127.0.0.1;Port=3307;Database=SalesDataWarehouse;User=NeftaliPC;";

            services.AddDbContext<StagingDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Registrar Repositorios
            services.AddScoped<IStagingRepository, StagingRepository>();

            // Registrar Extractors (Strategy Pattern)
            services.AddScoped<IDataExtractor<StagingCustomer>, CsvCustomerExtractor>();
            services.AddScoped<IDataExtractor<StagingProduct>, ApiProductExtractor>();
            services.AddScoped<IDataExtractor<StagingOrder>, DatabaseOrderExtractor>();

            // Registrar HttpClient para API
            services.AddHttpClient<ApiProductExtractor>();

            // Registrar Casos de Uso
            services.AddScoped<ExtractDataUseCase>();

            // Registrar Worker Service
            services.AddHostedService<EtlWorker>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.Information("Aplicación finalizada");
    Log.CloseAndFlush();
}