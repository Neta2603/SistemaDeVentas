using Microsoft.EntityFrameworkCore;
using SDV.Core.Entities.Dimensions;
using SDV.Core.Entities.Facts;
using SDV.Core.Entities.Staging;

namespace SDV.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para el Data Warehouse
/// Incluye tablas Staging y Dimensiones
/// </summary>
public class StagingDbContext : DbContext
{
    public StagingDbContext(DbContextOptions<StagingDbContext> options) : base(options)
    {
    }

    // ==================== Tablas Staging ====================
    public DbSet<StagingCustomer> StagingCustomers { get; set; }
    public DbSet<StagingProduct> StagingProducts { get; set; }
    public DbSet<StagingOrder> StagingOrders { get; set; }
    public DbSet<StagingOrderDetail> StagingOrderDetails { get; set; }

    // ==================== Tablas Dimensiones ====================
    public DbSet<DimCustomer> DimCustomers { get; set; }
    public DbSet<DimProduct> DimProducts { get; set; }
    public DbSet<DimTime> DimTimes { get; set; }
    public DbSet<DimStatus> DimStatuses { get; set; }

    // ==================== Tablas de Hechos ====================
    public DbSet<FactSales> FactSales { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==================== Configuración Staging ====================
        
        // Configuración de StagingCustomers
        modelBuilder.Entity<StagingCustomer>(entity =>
        {
            entity.ToTable("StagingCustomers");
            entity.HasKey(e => new { e.CustomerID, e.LoadDate });
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.LoadDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configuración de StagingProducts
        modelBuilder.Entity<StagingProduct>(entity =>
        {
            entity.ToTable("StagingProducts");
            entity.HasKey(e => new { e.ProductID, e.LoadDate });
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.LoadDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configuración de StagingOrders
        modelBuilder.Entity<StagingOrder>(entity =>
        {
            entity.ToTable("StagingOrders");
            entity.HasKey(e => new { e.OrderID, e.LoadDate });
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.LoadDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configuración de StagingOrderDetails
        modelBuilder.Entity<StagingOrderDetail>(entity =>
        {
            entity.ToTable("StagingOrderDetails");
            entity.HasKey(e => new { e.OrderID, e.ProductID, e.LoadDate });
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.LoadDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ==================== Configuración Dimensiones ====================

        // Configuración de DimCustomer
        modelBuilder.Entity<DimCustomer>(entity =>
        {
            entity.ToTable("DimCustomer");
            entity.HasKey(e => e.CustomerKey);
            entity.Property(e => e.CustomerKey).ValueGeneratedOnAdd();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.EndDate).HasDefaultValue(new DateTime(9999, 12, 31));
            entity.Property(e => e.IsCurrent).HasDefaultValue(true);
            
            // Índices
            entity.HasIndex(e => e.CustomerID).HasDatabaseName("idx_customerid");
            entity.HasIndex(e => e.IsCurrent).HasDatabaseName("idx_iscurrent");
            entity.HasIndex(e => new { e.StartDate, e.EndDate }).HasDatabaseName("idx_dates");
        });

        // Configuración de DimProduct
        modelBuilder.Entity<DimProduct>(entity =>
        {
            entity.ToTable("DimProduct");
            entity.HasKey(e => e.ProductKey);
            entity.Property(e => e.ProductKey).ValueGeneratedOnAdd();
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.StartDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.EndDate).HasDefaultValue(new DateTime(9999, 12, 31));
            entity.Property(e => e.IsCurrent).HasDefaultValue(true);
            
            // Índices
            entity.HasIndex(e => e.ProductID).HasDatabaseName("idx_productid");
            entity.HasIndex(e => e.Category).HasDatabaseName("idx_category");
            entity.HasIndex(e => e.IsCurrent).HasDatabaseName("idx_iscurrent_product");
            entity.HasIndex(e => new { e.StartDate, e.EndDate }).HasDatabaseName("idx_dates_product");
        });

        // Configuración de DimTime
        modelBuilder.Entity<DimTime>(entity =>
        {
            entity.ToTable("DimTime");
            entity.HasKey(e => e.TimeKey);
            entity.Property(e => e.TimeKey).ValueGeneratedNever(); // YYYYMMDD format
            entity.Property(e => e.MonthName).HasMaxLength(20);
            entity.Property(e => e.DayName).HasMaxLength(20);
            entity.Property(e => e.IsWeekend).HasDefaultValue(false);
            entity.Property(e => e.IsHoliday).HasDefaultValue(false);
            
            // Índices
            entity.HasIndex(e => e.FullDate).HasDatabaseName("idx_fulldate");
            entity.HasIndex(e => new { e.Year, e.Month }).HasDatabaseName("idx_year_month");
            entity.HasIndex(e => new { e.Year, e.Quarter }).HasDatabaseName("idx_year_quarter");
        });

        // Configuración de DimStatus
        modelBuilder.Entity<DimStatus>(entity =>
        {
            entity.ToTable("DimStatus");
            entity.HasKey(e => e.StatusKey);
            entity.Property(e => e.StatusKey).ValueGeneratedOnAdd();
            entity.Property(e => e.StatusName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.StatusDescription).HasMaxLength(200);
            
            // Índice único en StatusName
            entity.HasIndex(e => e.StatusName).IsUnique().HasDatabaseName("idx_statusname");
        });

        // ==================== Configuración Tabla de Hechos ====================

        // Configuración de FactSales
        modelBuilder.Entity<FactSales>(entity =>
        {
            entity.ToTable("FactSales");
            entity.HasKey(e => e.SalesKey);
            entity.Property(e => e.SalesKey).ValueGeneratedOnAdd();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.LoadDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Índices para optimización de consultas
            entity.HasIndex(e => e.CustomerKey).HasDatabaseName("idx_customerkey");
            entity.HasIndex(e => e.ProductKey).HasDatabaseName("idx_productkey");
            entity.HasIndex(e => e.TimeKey).HasDatabaseName("idx_timekey");
            entity.HasIndex(e => e.StatusKey).HasDatabaseName("idx_statuskey");
            entity.HasIndex(e => e.OrderID).HasDatabaseName("idx_orderid");
            entity.HasIndex(e => new { e.TimeKey, e.CustomerKey }).HasDatabaseName("idx_time_customer");
            entity.HasIndex(e => new { e.TimeKey, e.ProductKey }).HasDatabaseName("idx_time_product");
        });
    }
}
