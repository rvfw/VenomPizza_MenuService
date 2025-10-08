namespace VenomPizzaMenuService.src.service;

public interface ICartsService
{
    Task AddProductToCart(int cartId, int id, int priceId, int quantity);
    Task UpdateProductQuantityInCart(int cartId, int id, int priceId, int quantity);
    Task DeleteProductInCart(int cartId, int id, int priceId);
}
