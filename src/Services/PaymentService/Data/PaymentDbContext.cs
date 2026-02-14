using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data;

/// <summary>
/// Database context for Payment Service
/// </summary>
public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes as per database design
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(p => p.OrderId)
                .HasDatabaseName("IDX_PaymentOrderId");
            
            entity.HasIndex(p => p.Status)
                .HasDatabaseName("IDX_PaymentStatus");
            
            entity.Property(p => p.Amount)
                .HasPrecision(18, 2);
            
            // Configure DateTime columns for MySQL
            entity.Property(p => p.CreatedAt)
                .HasColumnType("datetime");
            
            entity.Property(p => p.ProcessedAt)
                .HasColumnType("datetime");
        });
    }
}
