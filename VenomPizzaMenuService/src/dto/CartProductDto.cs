namespace VenomPizzaMenuService.src.dto;

public class CartProductDto
{

    public int CartId { get; set; }
    public int Id { get; set; }
    public int Quantity { get; set; }

    public CartProductDto(int cartId, int id, int quantity=1)
    {
        CartId = cartId;
        Id = id;
        Quantity = quantity;
    }
}
