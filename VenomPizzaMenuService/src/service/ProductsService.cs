using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;
using VenomPizzaMenuService.src.repository;

namespace VenomPizzaMenuService.src.service;

public class ProductsService
{
    private readonly ProductsRepository productsRepository;
    public ProductsService(ProductsRepository productsRepository)
    {
        this.productsRepository = productsRepository;
    }

    #region create
    public async Task<Product> AddProduct(int id, ProductDto newProduct)
    {
        return await productsRepository.AddProduct(id,newProduct);
    }

    public async Task<Product> AddProduct(int id, ComboDto newCombo)
    {
        return await productsRepository.AddProduct(id, newCombo);
    }
    #endregion

    #region read
    public async Task<Product> GetProductById(int id)
    {
        return await productsRepository.GetProductById(id);
    }
    public async Task<List<Product>> GetProductsPage(int page,int size)
    {
        var foundedProducts= await productsRepository.GetProductsPage(page,size);
        return foundedProducts;
    }
    #endregion

    #region update
    public async Task<Product> UpdateProductInfo(int id, ProductDto updatedProduct)
    {
        return await productsRepository.UpdateProductInfo(id,updatedProduct);
    }
    #endregion

    #region delete
    public void DeleteProductById(int id)
    {
        productsRepository.DeleteProductById(id);
    }
    #endregion
}
