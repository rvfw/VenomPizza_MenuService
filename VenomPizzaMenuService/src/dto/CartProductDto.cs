namespace VenomPizzaMenuService.src.dto;

public class CartProductDto
{

    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public CartProductDto(int cartId, int productId, int quantity=1)
    {
        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
    }
}
