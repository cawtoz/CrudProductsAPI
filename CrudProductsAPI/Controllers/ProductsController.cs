using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrudProductsAPI.Data;
using CrudProductsAPI.Models;
using CrudProductsAPI.DTOs;

namespace CrudProductsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
    {

        try
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los productos");
            return StatusCode(500, new ErrorResponse { Message = "Error al obtener los productos" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductDto productDto)
    {
        try
        {
            if (productDto == null)
            {
                return BadRequest(new ErrorResponse { Message = "Debes enviar información en el cuerpo de la solicitud" });
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
                return BadRequest(new ErrorResponse { Message = errors });
            }

            var product = new Product
            {
                Name = productDto.Name.Trim(),
                Description = productDto.Description.Trim(),
                Price = productDto.Price
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return StatusCode(201, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el producto");
            return StatusCode(500, new ErrorResponse { Message = "Error al crear el producto" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<object>> DeleteProduct(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new ErrorResponse { Message = "El ID debe ser un número positivo" });
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new ErrorResponse { Message = "Producto no encontrado" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Producto eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el producto");
            return StatusCode(500, new ErrorResponse { Message = "Error al eliminar el producto" });
        }
    }
}
