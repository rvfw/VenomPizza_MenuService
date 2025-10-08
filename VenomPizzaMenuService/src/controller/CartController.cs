using Microsoft.AspNetCore.Mvc;
using VenomPizzaMenuService.src.attribute;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuService.src.controller;

public class CartController : Controller
{
    private readonly ICartsService _cartsService;
    public CartController(ICartsService productService)
    {
        _cartsService = productService;
    }

    [HttpPost("{id}")]
    [ValidateUserId]
    public async Task<IActionResult> AddProductToCart([FromRoute] int id, [FromQuery] int priceId, [FromQuery] int quantity = 1)
    {
        if (quantity < 1)
            return BadRequest("Кол-во продукта должно быть больше 0");
        int userId = (int)HttpContext.Items["Id"]!;
        try
        {
            await _cartsService.AddProductToCart(userId, id, priceId, quantity);
            return Accepted();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [ValidateUserId]
    public async Task<IActionResult> UpdateProductQuantityInCart([FromRoute] int id, [FromQuery] int priceId, [FromQuery] int quantity = 1)
    {
        if (quantity < 1)
            return BadRequest("Кол-во продукта должно быть больше 0");

        int userId = (int)HttpContext.Items["Id"]!;
        try
        {
            await _cartsService.UpdateProductQuantityInCart(userId, id, priceId, quantity);
            return Accepted();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [ValidateUserId]
    public async Task<IActionResult> DeleteProductInCart([FromRoute] int id, [FromQuery] int priceId)
    {
        int userId = (int)HttpContext.Items["Id"]!;
        try
        {
            await _cartsService.DeleteProductInCart(userId, id, priceId);
            return Accepted();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
