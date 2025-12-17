using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly DataContext _context;

    public ProductsController(ILogger<ProductsController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET /products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // GET /products/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    // POST /products
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        // Validate attributes
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        // Audit fields
        product.CreatedDate = DateTime.UtcNow;
        product.LastUpdatedDate = DateTime.UtcNow;

        _context.Products.Add(product);
        var success = await _context.SaveChangesAsync() > 0;

        if (!success) return BadRequest("Failed to create product");
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT /products/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest("ID in URL and body must match");

        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        var existing = await _context.Products.FindAsync(id);
        if (existing == null) return NotFound();

        // Update fields
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.IsOnSale = product.IsOnSale;
        existing.SalePrice = product.SalePrice;
        existing.CurrentStock = product.CurrentStock;
        existing.ImageUrl = product.ImageUrl;
        existing.LastUpdatedDate = DateTime.UtcNow;

        var success = await _context.SaveChangesAsync() > 0;
        if (!success) return BadRequest("Failed to update product");
        return Ok(existing);
    }

    // DELETE /products/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        var success = await _context.SaveChangesAsync() > 0;
        if (!success) return BadRequest("Failed to delete product");
        return NoContent();
    }

    // GET /products/search?name=...&minPrice=...&maxPrice=...&isOnSale=...&inStock=...&sortBy=...&sortOrder=...
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(
        [FromQuery] string? name = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isOnSale = null,
        [FromQuery] bool? inStock = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var lower = name.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(lower));
        }

        if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
        if (isOnSale.HasValue) query = query.Where(p => p.IsOnSale == isOnSale.Value);
        if (inStock.HasValue && inStock.Value) query = query.Where(p => p.CurrentStock > 0);

        // Execute the query first (for SQLite compatibility), then do in-memory sorting
        var products = await query.ToListAsync();

        products = sortBy.ToLower() switch
        {
            "price" => sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? products.OrderByDescending(p => p.Price).ToList()
                : products.OrderBy(p => p.Price).ToList(),

            "created" => sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? products.OrderByDescending(p => p.CreatedDate).ToList()
                : products.OrderBy(p => p.CreatedDate).ToList(),

            "stock" => sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? products.OrderByDescending(p => p.CurrentStock).ToList()
                : products.OrderBy(p => p.CurrentStock).ToList(),

            _ => sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? products.OrderByDescending(p => p.Name).ToList()
                : products.OrderBy(p => p.Name).ToList()
        };

        return Ok(products);
    }
}
