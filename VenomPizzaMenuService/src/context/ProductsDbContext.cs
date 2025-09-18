using Microsoft.EntityFrameworkCore;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.context;

public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Combo> Combos { get; set; }
    public DbSet<ComboProduct> ComboProducts { get; set; }
    public DbSet<PriceVariant> PriceVariants { get; set; }

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
        modelBuilder.Entity<PriceVariant>()
            .HasOne(pv => pv.Product)
            .WithMany(p => p.PriceVariants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PriceVariant>()
            .HasKey(pv => new {pv.ProductId,pv.PriceId});
    }
}
