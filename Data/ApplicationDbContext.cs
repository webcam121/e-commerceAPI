using Microsoft.EntityFrameworkCore;
using ecommerceAPI.Models;

namespace ecommerceAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryAttribute> CategoryAttributes { get; set; }
    public DbSet<AttributeValue> AttributeValues { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ProductAttributeValue as a many-to-many relationship
        modelBuilder.Entity<ProductAttributeValue>()
            .HasKey(pav => new { pav.ProductId, pav.AttributeValueId });

        modelBuilder.Entity<ProductAttributeValue>()
            .HasOne(pav => pav.Product)
            .WithMany(p => p.AttributeValues)
            .HasForeignKey(pav => pav.ProductId);

        modelBuilder.Entity<ProductAttributeValue>()
            .HasOne(pav => pav.AttributeValue)
            .WithMany(av => av.ProductAttributeValues)
            .HasForeignKey(pav => pav.AttributeValueId);

        // Configure CategoryAttribute as a many-to-many relationship with Category
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Attributes)
            .WithMany(a => a.Categories)
            .UsingEntity(j => j.ToTable("CategoryAttributeCategories"));

        // Configure AttributeValue
        modelBuilder.Entity<AttributeValue>()
            .HasOne(av => av.CategoryAttribute)
            .WithMany(ca => ca.Values)
            .HasForeignKey(av => av.CategoryAttributeId);

        // Configure Product
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
    }
} 