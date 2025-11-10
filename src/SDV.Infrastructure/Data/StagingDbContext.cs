using Microsoft.EntityFrameworkCore;
using SDV.Core.Entities.Staging;

namespace SDV.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos para tablas staging
/// </summary>
public class StagingDbContext : DbContext
{
    public StagingDbContext(DbContextOptions<StagingDbContext> options) : base(options)
    {
    }

    public DbSet<StagingCustomer> StagingCustomers { get; set; }
    public DbSet<StagingProduct> StagingProducts { get; set; }
    public DbSet<StagingOrder> StagingOrders { get; set; }
    public DbSet<StagingOrderDetail> StagingOrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}