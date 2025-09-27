using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.model;

namespace VenomPizzaMenuService.src.repository;

public interface IProductsRepository
{
    Task<Product> AddProduct(ProductDto newProduct);
    Task<Product> AddProduct(ComboDto newCombo);
    Task<List<ProductShortInfoDto>?> GetProductsPage(int page, int size);
    Task<List<ProductShortInfoDto>?> GetProductsByCategory(string categoryName);
    Task<Product?> GetProductById(int id);
    Task<(int id, string title)?> GetProductIdAndTitle(int productId, int priceId);
    Task<Product> UpdateProductInfo(ProductDto updatedProduct);
    Task<bool> DeleteProductById(int id);
    Task CheckComboProducts(ComboDto newCombo);
}