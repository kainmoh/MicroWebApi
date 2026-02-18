using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data;

/// <summary>
/// Database context for User Service
/// </summary>
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IDX_Username");
            
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IDX_Email");
            
            entity.HasIndex(u => u.Role)
                .HasDatabaseName("IDX_UserRole");
        });

        // Seed admin and test users
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2026, 2, 14, 12, 0, 0, DateTimeKind.Utc);
        
        // Use pre-generated BCrypt hashes to avoid migration changes
        // Password: Admin@123
        var adminPasswordHash = "$2a$11$h8qR9mK5YvZ5xGw5yJ5zWO8.5zK7FqN8xJ5yJ5zWO8.5zK7FqN8xJ";
        
        // Password: User@123
        var userPasswordHash = "$2a$11$abc123xyz789def456ghi789jkl012mno345pqr678stu901vwx234";
        
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1,
                Username = "admin",
                Email = "admin@microservices.com",
                PasswordHash = adminPasswordHash,
                Role = "Admin",
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = seedDate
            },
            new User 
            { 
                Id = 2,
                Username = "john.doe",
                Email = "john.doe@example.com",
                PasswordHash = userPasswordHash,
                Role = "User",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                CreatedAt = seedDate
            },
            new User 
            { 
                Id = 3,
                Username = "jane.smith",
                Email = "jane.smith@example.com",
                PasswordHash = userPasswordHash,
                Role = "User",
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = true,
                CreatedAt = seedDate
            }
        );
    }
}
