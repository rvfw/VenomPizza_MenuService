using Microsoft.AspNetCore.Mvc.RazorPages;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.src.service;

public class ProductsService
{
    private readonly ProductsDbContext dbContext;
    public ProductsService(ProductsDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    #region create
    public Product AddProduct(int id, ProductDto newProduct)
    {
        return dbContext.AddProduct(id,newProduct);
    }

    public Product AddProduct(int id, ComboDto newCombo)
    {
        return dbContext.AddProduct(id, newCombo);
    }
    #endregion

    #region read
    public Product GetProductById(int id)
    {
        return dbContext.GetProductById(id);
    }
    public IEnumerable<Product> GetProductsPage(int page,int size)
    {
        return dbContext.GetProductsPage(page,size);
    }
    #endregion

    #region update
    public Product UpdateProductInfo(int id, ProductDto updatedProduct)
    {
        return dbContext.UpdateProductInfo(id,updatedProduct);
    }
    #endregion

    #region delete
    public void DeleteProductById(int id)
    {
        dbContext.DeleteProductById(id);
    }
    #endregion
}
