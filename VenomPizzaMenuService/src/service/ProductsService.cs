using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.src.service;

public class ProductsService:IProductsService
{
    private readonly ProductsRepository productsRepository;
    public ProductsService(ProductsRepository productsRepository)
    {
        this.productsRepository = productsRepository;
    }

    #region create
    public async Task<Product> AddProduct(ProductDto newProduct)
    {
        newProduct.Validate();
        return await productsRepository.AddProduct(newProduct);
    }

    public async Task<Product> AddProduct(ComboDto newCombo)
    {
        newCombo.Validate();
        return await productsRepository.AddProduct(newCombo);
    }
    #endregion

    #region read
    public async Task<Product> GetProductById(int id)
    {
        return await productsRepository.GetProductById(id);
    }
    public async Task<List<Product>> GetProductsPage(int page,int size)
    {
        return await productsRepository.GetProductsPage(page,size);
    }
    #endregion

    #region update
    public async Task<Product> UpdateProductInfo(ProductDto updatedProduct)
    {
        updatedProduct.Validate();
        return await productsRepository.UpdateProductInfo(updatedProduct);
    }
    #endregion

    #region delete
    public async Task DeleteProductById(int id)
    {
        await productsRepository.DeleteProductById(id);
    }
    #endregion
}
