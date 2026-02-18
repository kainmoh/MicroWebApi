using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

/// <summary>
/// Database context for Product Service
/// </summary>
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes as per database design
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Name)
                .HasDatabaseName("IDX_ProductName");
            
            entity.HasIndex(p => p.Category)
                .HasDatabaseName("IDX_ProductCategory");
            
            entity.Property(p => p.Price)
                .HasPrecision(18, 2);
            
            // Configure DateTime columns for MySQL
            entity.Property(p => p.CreatedAt)
                .HasColumnType("datetime");
            
            entity.Property(p => p.UpdatedAt)
                .HasColumnType("datetime");
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 2, 14, 8, 31, 45, 301, DateTimeKind.Utc).AddTicks(6482);
        
        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                Name = "Laptop", 
                Description = "High-performance gaming laptop with RTX 4080", 
                Price = 1500.00m, 
                Quantity = 50, 
                Category = "Electronics",
                CreatedAt = seedDate
            },
            new Product 
            { 
                Id = 2, 
                Name = "Wireless Mouse", 
                Description = "Ergonomic wireless mouse with precision tracking", 
                Price = 25.00m, 
                Quantity = 200, 
                Category = "Electronics",
                CreatedAt = seedDate
            },
            new Product 
            { 
                Id = 3, 
                Name = "Mechanical Keyboard", 
                Description = "RGB mechanical keyboard with Cherry MX switches", 
                Price = 100.00m, 
                Quantity = 150, 
                Category = "Electronics",
                CreatedAt = seedDate
            },
            new Product 
            { 
                Id = 4, 
                Name = "USB-C Hub", 
                Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader", 
                Price = 45.00m, 
                Quantity = 300, 
                Category = "Accessories",
                CreatedAt = seedDate
            },
            new Product 
            { 
                Id = 5, 
                Name = "Webcam HD", 
                Description = "1080p HD webcam with auto-focus", 
                Price = 75.00m, 
                Quantity = 120, 
                Category = "Electronics",
                CreatedAt = seedDate
            }
        );
    }
}
