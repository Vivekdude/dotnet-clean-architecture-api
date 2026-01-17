using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Name);
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Country);
            entity.Ignore(e => e.FullName);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop Pro 15", Description = "High-performance laptop with 15-inch display", Price = 1299.99m, Category = "Electronics", StockQuantity = 50, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with precision tracking", Price = 49.99m, Category = "Electronics", StockQuantity = 200, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI and card reader", Price = 79.99m, Category = "Electronics", StockQuantity = 150, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "Office Chair", Description = "Ergonomic office chair with lumbar support", Price = 299.99m, Category = "Furniture", StockQuantity = 30, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Id = 5, Name = "Standing Desk", Description = "Electric standing desk with memory presets", Price = 599.99m, Category = "Furniture", StockQuantity = 20, IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+1-555-0101", Address = "123 Main St", City = "New York", Country = "USA", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Phone = "+1-555-0102", Address = "456 Oak Ave", City = "Los Angeles", Country = "USA", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Customer { Id = 3, FirstName = "Michael", LastName = "Johnson", Email = "michael.johnson@example.com", Phone = "+44-20-1234-5678", Address = "789 Baker St", City = "London", Country = "UK", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }
}
