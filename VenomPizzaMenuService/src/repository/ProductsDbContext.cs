using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.repository;

public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    private DbSet<Product> Products { get; set; }
    private DbSet<Dish> Dishes { get; set; }
    private DbSet<Combo> Combos { get; set; }
    private DbSet<ComboProduct> ComboProducts { get; set; }

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

    #region create
    public Product AddProduct(int id,ProductDto newProduct)
    {
        if (GetProductById(id) != null)
            throw new BadHttpRequestException("Товар с ID " + id + " уже существует");
        var addedProduct= Products.Add(new Product(newProduct));
        SaveChanges();
        return addedProduct.Entity;
    }

    public Product AddProduct(int id, ComboDto newCombo)
    {
        if (GetProductById(id) != null)
            throw new BadHttpRequestException("Комбо с ID " + id + " уже существует");
        var addedCombo = Products.Add(new Combo(newCombo));
        SaveChanges();
        foreach(var idAndCount in newCombo.ProductsDict)
            ComboProducts.Add(new ComboProduct(addedCombo.Entity.Id, idAndCount.Key, idAndCount.Value));
        return addedCombo.Entity;
    }
    #endregion

    #region read
    public IEnumerable<Product> GetProductsPage(int page, int size)
    {
        if (Products.Count() < page * size)
            throw new KeyNotFoundException("Страницы " + page + " не существует");
        return Products.Skip(page * size).Take(size);
    }

    public Product GetProductById(int id)
    {
        var foundedProduct = Products.FirstOrDefault(x => x.Id == id);
        if (foundedProduct == null)
            throw new KeyNotFoundException("Товара с ID " + id + " не найдено");
        return foundedProduct;
    }
    #endregion

    #region update
    public Product UpdateProductInfo(int id, ProductDto updatedProduct)
    {
        var foundedProduct = GetProductById(id);
        if(foundedProduct==null)
            throw new KeyNotFoundException("Товара с ID " + id + " не найдено");
        Entry(foundedProduct).CurrentValues.SetValues(updatedProduct);
        if (foundedProduct is Dish foundedDish && updatedProduct is DishDto updatedDish)
            UpdateDish(foundedDish, updatedDish);
        else if (foundedProduct is Combo foundedCombo && updatedProduct is ComboDto updatedCombo)
            UpdateCombo(foundedCombo, updatedCombo);
        SaveChanges();
        return foundedProduct;
    }
    #endregion

    #region delete
    public void DeleteProductById(int id)
    {
        var foundedProduct= GetProductById(id);
        if (foundedProduct == null)
            throw new KeyNotFoundException("Товара с ID " + id + " не найдено");
        Products.Remove(foundedProduct);
        SaveChanges();
    }
    #endregion

    #region private
    private void DeleteComboProducts(Combo combo)
    {
        foreach(var product in combo.Products)
            ComboProducts.Remove(product);
        SaveChanges();
    }
    private void UpdateDish(Dish subject,DishDto dto)
    {
        subject.Proteins = dto.Proteins;
        subject.Fats = dto.Fats;
        subject.Carbohydrates = dto.Carbohydrates;
        subject.Calorific = dto.Calorific;
        subject.Ingredients = dto.Ingredients;
        subject.Allergens = dto.Allergens;
    }
    private void UpdateCombo(Combo subject,ComboDto dto)
    {
        if (dto.Products != null)
        {
            DeleteComboProducts(subject);
            foreach (var idAndCount in dto.ProductsDict)
                ComboProducts.Add(new ComboProduct(subject.Id, idAndCount.Key, idAndCount.Value));
        }
    }
    #endregion
}
