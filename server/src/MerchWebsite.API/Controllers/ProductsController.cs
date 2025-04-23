// server/src/MerchWebsite.API/Controllers/ProductsController.cs
using MerchWebsite.API.Data;
using MerchWebsite.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // <<< Add using for LINQ methods like Where

namespace MerchWebsite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // --- MODIFY GetProducts Method ---
        // GET: api/products?category=SomeCategory
        [HttpGet]
        // Add category query parameter (optional)
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] string? category) // <<< Add parameter
        {
            // Start with the base query for all products
            var query = _context.Products.AsQueryable(); // Start as IQueryable

            // Check if a category filter was provided in the query string
            if (!string.IsNullOrEmpty(category))
            {
                Console.WriteLine($"API: Filtering products by category: {category}"); // Add log
                // Apply a WHERE clause to filter by the category
                // Use case-insensitive comparison for robustness
                query = query.Where(p => EF.Functions.ILike(p.Category, category));
                // Alternatively, for exact match (case-sensitive depending on DB collation):
                // query = query.Where(p => p.Category == category);
            }
            else
            {
                Console.WriteLine($"API: Getting all products (no category filter)."); // Add log
            }

            // Execute the query (which may or may not have the Where clause)
            var products = await query.ToListAsync();

            return Ok(products);
        }
        // --- END MODIFY GetProducts ---


        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            // ... (existing GetProduct code remains the same) ...
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }
}