using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.service;

public interface IProductsService
{
    Task<Product> AddProduct(ProductDto newProduct);
    Task<Product> GetProductById(int id);
    Task<List<ProductShortInfoDto>> GetProductsPage(int page, int size);
    Task<List<ProductShortInfoDto>> GetProductsByCategory(string categoryName);
    Task<Product> UpdateProductInfo(ProductDto updatedProduct);
    Task DeleteProductById(int id);
    Task AddProductToCart(int cartId, int id, int priceId, int quantity);
    Task UpdateProductQuantityInCart(int cartId, int id, int priceId, int quantity);
    Task DeleteProductInCart(int cartId, int id, int priceId);
}
