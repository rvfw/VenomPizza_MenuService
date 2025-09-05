using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.service;

public interface IProductsService
{
    Task<Product> AddProduct(ProductDto newProduct);
    Task<Product> GetProductById(int id);
    Task<List<ProductInMenuDto>> GetProductsPage(int page, int size);
    Task<Product> UpdateProductInfo(ProductDto updatedProduct);
    Task DeleteProductById(int id);
}
