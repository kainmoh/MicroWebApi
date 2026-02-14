using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data;

/// <summary>
/// Database context for Order Service
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes as per database design
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.Status)
                .HasDatabaseName("IDX_OrderStatus");
            
            entity.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("IDX_OrderCreatedAt");
            
            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);
            
            // Configure DateTime columns for MySQL
            entity.Property(o => o.CreatedAt)
                .HasColumnType("datetime");
            
            entity.Property(o => o.CompletedAt)
                .HasColumnType("datetime");
        });
    }
}
