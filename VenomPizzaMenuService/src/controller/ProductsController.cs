using Microsoft.AspNetCore.Mvc;
using VenomPizzaMenuService.src.attribute;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuService.src.controller;
[ApiController]
[Route("api/products")]
public class ProductsController : Controller
{
    private readonly ProductsService _productsService;
    public ProductsController(ProductsService productsService)
    {
        this._productsService = productsService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById([FromRoute]int id)
    {
        try
        {
            return Ok(await _productsService.GetProductById(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetProductsPage([FromQuery]int page=1, [FromQuery] int size = 50)
    {
        try
        {
            return Ok(await _productsService.GetProductsPage(page-1,size));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(ProductDto dto)
    {
        try
        {
            return Ok(await _productsService.AddProduct(dto));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex);
        }
    }

    [ValidateUserId]
    [HttpPost("{id}")]
    public async Task<IActionResult> AddProductToCart([FromRoute] int id, [FromQuery]int quantity=1)
    {
        if(id<0)
            return BadRequest("Id продукта должно быть положительным");
        if(quantity<1)
            return BadRequest("Кол-во продукта должно быть больше 0");
        int userId = (int)HttpContext.Items["Id"]!;
        await _productsService.AddProductToCart(userId, id, quantity);
        return Accepted();
    }

    [HttpPut("{id}")]
    [ValidateUserId]
    public async Task<IActionResult> UpdateProductQuantityInCart([FromRoute] int id, [FromQuery] int quantity = 1)
    {
        if (id < 0)
            return BadRequest("Id продукта должно быть положительным");
        if (quantity < 1)
            return BadRequest("Кол-во продукта должно быть больше 0");
        int userId = (int)HttpContext.Items["Id"]!;
        await _productsService.UpdateProductQuantityInCart(userId, id, quantity);
        return Accepted();
    }

    [HttpDelete("{id}")]
    [ValidateUserId]
    public async Task<IActionResult> DeleteProductInCart([FromRoute] int id)
    {
        if (id < 0)
            return BadRequest("Id продукта должно быть положительным");
        int userId = (int)HttpContext.Items["Id"]!;
        await _productsService.DeleteProductInCart(userId, id);
        return Accepted();
    }
}
