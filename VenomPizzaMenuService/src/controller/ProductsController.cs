using Microsoft.AspNetCore.Mvc;
using VenomPizzaMenuService.src.attribute;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuService.src.controller;
[ApiController]
[Route("api/products")]
public class ProductsController : Controller
{
    private readonly IProductsService _productsService;
    public ProductsController(IProductsService productsService)
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
    }

    [HttpGet("category/{categoryName}")]
    public async Task<IActionResult> GetProductsByCategory([FromRoute] string categoryName)
    {
        try
        {
            return Ok(await _productsService.GetProductsByCategory(categoryName));
        }
        catch(KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    #region productUpdateTemp
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
    [HttpPut]
    public async Task<IActionResult> UpdateProduct(ProductDto dto)
    {
        try
        {
            return Ok(await _productsService.UpdateProductInfo(dto));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex);
        }
    }
    [HttpDelete]
    public async Task<IActionResult> DeleteProduct([FromQuery] int id)
    {
        try
        {
            await _productsService.DeleteProductById(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex);
        }
    } 
    #endregion

    [HttpPost("{id}")]
    [ValidateUserId]
    public async Task<IActionResult> AddProductToCart([FromRoute] int id, [FromQuery]int priceId, [FromQuery]int quantity=1)
    {
        if(quantity<1)
            return BadRequest("Кол-во продукта должно быть больше 0");
        int userId = (int)HttpContext.Items["Id"]!;
        try
        {
            await _productsService.AddProductToCart(userId, id, priceId, quantity);
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
            await _productsService.UpdateProductQuantityInCart(userId, id, priceId, quantity);
            return Accepted();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [ValidateUserId]
    public async Task<IActionResult> DeleteProductInCart([FromRoute] int id,[FromQuery] int priceId)
    {
        int userId = (int)HttpContext.Items["Id"]!;
        try
        {
            await _productsService.DeleteProductInCart(userId, id, priceId);
            return Accepted();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
