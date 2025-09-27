using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.repository;

public class ProductsRepository
{
    private readonly ProductsDbContext dbContext;
    public ProductsRepository(ProductsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    #region create
    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var addedProduct = await AddProductToDatabase(newProduct);
            var priceVariants = newProduct.PriceVariantsDict.Select((x, y) => new PriceVariant(addedProduct, y, x.Key, x.Value));
            dbContext.PriceVariants.AddRange(priceVariants);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return addedProduct;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Product> AddProduct(ComboDto newCombo)
    {
        await using var transaction=await dbContext.Database.BeginTransactionAsync();
        try
        {
            var idList = newCombo.ProductsDict.Keys.ToList();
            var foundedProducts = await dbContext.Products.Where(x => idList.Contains(x.Id)).ToListAsync();
            foreach (var searchedProduct in newCombo.ProductsDict)
                if (!foundedProducts.Any(x => x.Id == searchedProduct.Key))
                    throw new ArgumentException("Товара с ID " + searchedProduct.Key + " не существует");

            var addedCombo = await AddProductToDatabase(newCombo);
            dbContext.ComboProducts.AddRange(newCombo.ProductsDict.Select(x => new ComboProduct(addedCombo.Id, x.Key, x.Value)));

            var priceVariants = newCombo.PriceVariantsDict.Select((x, y) => new PriceVariant(addedCombo, y, x.Key, x.Value));
            dbContext.PriceVariants.AddRange(priceVariants);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return addedCombo;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    #endregion

    #region read
    public async Task<List<ProductShortInfoDto>> GetProductsPage(int page, int size)
    {
        var foundedProducts = await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip(page * size)
            .Take(size)
            .Select(p => new ProductShortInfoDto(p))
            .ToListAsync();
        if (foundedProducts.Count() == 0)
            throw new KeyNotFoundException("Страницы " + (page+1) + " не существует");
        return foundedProducts;
    }

    public async Task<Product> GetProductById(int id)
    {
        var foundedProduct= await dbContext.Products
            .Include(p=>(p as Combo).Products)
            .ThenInclude(cp=>cp.Product)
            .Include(p=>p.PriceVariants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (foundedProduct == null)
            throw new KeyNotFoundException("Товара с ID " + id + " не найдено");
        return foundedProduct;
    }

    public async Task<(int id,string title)> GetProductIdAndTitle(int productId,int priceId)
    {
        var foundedProduct=await dbContext.Products
            .Include(x=>x.PriceVariants)
            .FirstOrDefaultAsync(x=>x.Id== productId);
        if (foundedProduct == null)
            throw new KeyNotFoundException($"Товар с Id {productId} не найден");
        if (foundedProduct.PriceVariants.FirstOrDefault(x => x.PriceId == priceId) == null)
            throw new KeyNotFoundException($"Цена {priceId} товара {productId} не найдена");
        return (foundedProduct.Id, foundedProduct.Title);
    }
    #endregion

    #region update
    public async Task<Product> UpdateProductInfo(ProductDto updatedProduct)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var foundedProduct = await GetProductById(updatedProduct.Id);
            if (foundedProduct == null)
                throw new KeyNotFoundException("Товара с ID " + updatedProduct.Id + " не найдено");

            dbContext.Entry(foundedProduct).CurrentValues.SetValues(updatedProduct);
            dbContext.PriceVariants.RemoveRange(foundedProduct.PriceVariants);

            var priceVariants = updatedProduct.PriceVariantsDict.Select((x, y) => new PriceVariant(foundedProduct, y, x.Key, x.Value));
            dbContext.PriceVariants.AddRange(priceVariants);

            if (foundedProduct is Dish foundedDish && updatedProduct is DishDto updatedDish)
                UpdateDish(foundedDish, updatedDish);
            else if (foundedProduct is Combo foundedCombo && updatedProduct is ComboDto updatedCombo)
                UpdateCombo(foundedCombo, updatedCombo);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return foundedProduct;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }
    #endregion

    #region delete
    public async Task DeleteProductById(int id)
    {
        var foundedProduct = await GetProductById(id);
        dbContext.Products.Remove(foundedProduct);
        await dbContext.SaveChangesAsync();
    }
    #endregion

    #region private
    private void UpdateDish(Dish subject, DishDto dto)
    {
        subject.Proteins = dto.Proteins;
        subject.Fats = dto.Fats;
        subject.Carbohydrates = dto.Carbohydrates;
        subject.Calorific = dto.Calorific;
        subject.Ingredients = dto.Ingredients;
        subject.Allergens = dto.Allergens;
    }
    private async Task UpdateCombo(Combo subject, ComboDto dto)
    {
        if (dto.Products == null)
            return;
        dbContext.ComboProducts.RemoveRange(subject.Products);
        dbContext.ComboProducts.AddRange(dto.ProductsDict.Select(x => new ComboProduct(subject.Id, x.Key, x.Value)));
    }
    private async Task<Product> AddProductToDatabase(ProductDto newProduct)
    {
        if (await dbContext.Products.AnyAsync(x => x.Id == newProduct.Id))
            throw new ArgumentException("Товар с ID " + newProduct.Id + " уже существует");
        var addedProduct = dbContext.Products.Add(newProduct.ToProduct());
        await dbContext.SaveChangesAsync();
        return addedProduct.Entity;
    }
    #endregion
}
