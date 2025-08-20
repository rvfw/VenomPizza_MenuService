using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.context;

public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Combo> Combos { get; set; }
    public DbSet<ComboProduct> ComboProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .UseTptMappingStrategy();
        modelBuilder.Entity<ComboProduct>(entity =>
        {
            entity.HasKey(cp => new { cp.ProductId, cp.ComboId });

            entity.HasOne(cp => cp.Product)
                .WithMany()
                .HasForeignKey(cp => cp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cp => cp.Combo)
                .WithMany(c => c.Products)
                .HasForeignKey(cp => cp.ComboId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
