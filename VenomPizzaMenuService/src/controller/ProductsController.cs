using Microsoft.AspNetCore.Mvc;
using VenomPizzaMenuService.src.service;

namespace VenomPizzaMenuService.src.controller;
[ApiController]
[Route("api")]
public class ProductsController : Controller
{
    private readonly ProductsService productsService;
    public ProductsController(ProductsService productsService)
    {
        this.productsService = productsService;
    }
    [HttpGet("product/{id}")]
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
    [HttpGet("products")]
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
}
