using Microsoft.AspNetCore.Mvc;
using VenomPizzaMenuService.src.dto;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuService.src.controller;
[ApiController]
[Route("api/products")]
public class ProductsController : Controller
{
    private readonly ProductsService productsService;
    public ProductsController(ProductsService productsService)
    {
        this.productsService = productsService;
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById([FromRoute]int id)
    {
        try
        {
            return Ok(await productsService.GetProductById(id));
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
        if (page < 1 || size < 1)
            return BadRequest("Номер страницы и ее размер должны быть больше 0");
        try
        {
            return Ok(await productsService.GetProductsPage(page-1,size));
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
            return Ok(await productsService.AddProduct(dto));
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex);
        }
    }
    [HttpPost("{id}")]
    public async Task<IActionResult> AddProductToCart([FromRoute] int id, [FromQuery]int quantity=1)
    {
        int userId =int.Parse( Request.Headers["Id"].ToString());
        await productsService.AddProductToCart(userId, id, quantity);
        return Accepted();
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductQuantityInCart([FromRoute] int id, [FromQuery] int quantity = 1)
    {
        int userId = int.Parse(Request.Headers["Id"].ToString());
        await productsService.UpdateProductQuantityInCart(userId, id, quantity);
        return Accepted();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductInCart([FromRoute] int id)
    {
        int userId = int.Parse(Request.Headers["Id"].ToString());
        await productsService.DeleteProductInCart(userId, id);
        return Accepted();
    }
}
