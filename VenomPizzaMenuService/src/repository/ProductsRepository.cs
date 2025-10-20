using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VenomPizzaMenuService.src.context;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.repository;

public class ProductsRepository : IProductsRepository
{
    private readonly ProductsDbContext _dbContext;
    public ProductsRepository(ProductsDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    #region create
    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var addedProduct = await AddProductToDatabase(newProduct);
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
        await using var transaction=await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var addedCombo = await AddProductToDatabase(newCombo);
            _dbContext.ComboProducts.AddRange(newCombo.ProductsDict.Select(x => new ComboProduct(addedCombo.Id, x.Key, x.Value)));
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
    public async Task<List<ProductShortInfoDto>?> GetProductsPage(int page, int size)
    {
        var foundedProducts = await _dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip(page * size)
            .Take(size)
            .Include(p=>p.PriceVariants)
            .Select(p => new ProductShortInfoDto(p))
            .ToListAsync();
        return foundedProducts;
    }
    
    public async Task<List<ProductShortInfoDto>?> GetProductsByCategory(string categoryName)
    {
        var result= await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Categories!=null && p.Categories.Contains(categoryName))
            .OrderBy(p => p.Id)
            .Include(p => p.PriceVariants)
            .Select(p=>new ProductShortInfoDto(p))
            .ToListAsync();
        return result;
    }

    public async Task<Product?> GetProductById(int id)
    {
        var foundedProduct= await _dbContext.Products
            .Include(p=>(p as Combo).Products)
            .ThenInclude(cp=>cp.Product)
            .Include(p=>p.PriceVariants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id);
        return foundedProduct;
    }

    public async Task<(int id,string title)?> GetProductIdAndTitle(int productId,int priceId)
    {
        var foundedProduct=await _dbContext.Products
            .AsNoTracking()
            .Include(x=>x.PriceVariants)
            .FirstOrDefaultAsync(x=>x.Id== productId);
        if (foundedProduct == null || foundedProduct.PriceVariants.FirstOrDefault(x => x.PriceId == priceId) == null)
            return null;
        return (foundedProduct.Id, foundedProduct.Title);
    }

    public async Task<bool> AllComboProductsExist(ComboDto newCombo)
    {
        var idList = newCombo.ProductsDict.Keys.ToList();
        var foundedProducts = await _dbContext.Products.Where(x => idList.Contains(x.Id)).ToListAsync();
        foreach (var searchedProduct in newCombo.ProductsDict)
            if (!foundedProducts.Any(x => x.Id == searchedProduct.Key))
                return false;
        return true;
    }

    public async Task<bool> ComboContainsCombo(ComboDto newCombo)
    {
        var idList = newCombo.ProductsDict.Keys.ToList();
        var foundedProducts = await _dbContext.Products.Where(x => idList.Contains(x.Id)).ToListAsync();
        foreach (var product in foundedProducts)
            if (product is Combo)
                return true;
        return false;
    }
    #endregion

    #region update
    public async Task<Product> UpdateProductInfo(ProductDto updatedProduct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var foundedProduct = await GetProductById(updatedProduct.Id);
            if (foundedProduct == null)
                throw new KeyNotFoundException("Товара с ID " + updatedProduct.Id + " не найдено");

            UpdateProduct(foundedProduct, updatedProduct);
            await _dbContext.SaveChangesAsync();
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
    public async Task<bool> DeleteProductById(int id)
    {
        var foundedProduct = await GetProductById(id);
        if (foundedProduct == null)
            return false;
        _dbContext.Products.Remove(foundedProduct);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    #endregion

    #region private
    private void UpdateProduct(Product subject, ProductDto newObject)
    {
        _dbContext.Entry(subject).CurrentValues.SetValues(newObject);
        _dbContext.PriceVariants.RemoveRange(subject.PriceVariants);

        var priceVariants = newObject.PriceVariantsDict.Select((x, y) => new PriceVariant(subject, y, x.Key, x.Value));
        _dbContext.PriceVariants.AddRange(priceVariants);

        if (subject is Dish foundedDish && newObject is DishDto updatedDish)
            UpdateDishFields(foundedDish, updatedDish);
        else if (subject is Combo foundedCombo && newObject is ComboDto updatedCombo)
            RecreateComboProducts(foundedCombo, updatedCombo);
    }

    private void UpdateDishFields(Dish subject, DishDto newObject)
    {
        subject.Proteins = newObject.Proteins;
        subject.Fats = newObject.Fats;
        subject.Carbohydrates = newObject.Carbohydrates;
        subject.Calorific = newObject.Calorific;
        subject.Ingredients = newObject.Ingredients;
        subject.Allergens = newObject.Allergens;
    }

    private void RecreateComboProducts(Combo subject, ComboDto dto)
    {
        if (dto.Products == null)
            return;
        _dbContext.ComboProducts.RemoveRange(subject.Products);
        _dbContext.ComboProducts.AddRange(dto.ProductsDict.Select(x => new ComboProduct(subject.Id, x.Key, x.Value)));
    }

    private async Task<Product> AddProductToDatabase(ProductDto newProduct)
    {
        var addedProduct = _dbContext.Products.Add(newProduct.ToProduct()).Entity;
        await _dbContext.SaveChangesAsync();

        var priceVariants = newProduct.PriceVariantsDict.Select((x, y) => new PriceVariant(addedProduct, y, x.Key, x.Value));
        _dbContext.PriceVariants.AddRange(priceVariants);
        await _dbContext.SaveChangesAsync();
        return addedProduct;
    }
    #endregion
}
