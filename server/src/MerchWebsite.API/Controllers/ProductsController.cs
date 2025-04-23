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
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
           [FromQuery] string? category,
           [FromQuery] string? sortBy) // <<< Add sortBy parameter back
        {
            // --- Add Diagnostic Logging ---
            Console.WriteLine($"API: Request QueryString: {Request.QueryString}"); // Log raw query string
            foreach (var item in Request.Query) // Log each key-value pair
            {
                Console.WriteLine($"API: Query Param: {item.Key} = {item.Value}");
            }
            Console.WriteLine($"API: Request received (parameters). Category: '{category}', SortBy: '{sortBy}'"); // Log parameters
                                                                                                                  // --- End Diagnostic Logging ---


            var query = _context.Products.AsQueryable();

            // Category Filtering (Keep existing logic)
            if (!string.IsNullOrEmpty(category))
            {
                Console.WriteLine($"API: Applying category filter: {category}");
                query = query.Where(p => EF.Functions.ILike(p.Category, category));
            }
            else
            {
                Console.WriteLine($"API: No category filter applied.");
            }

            // --- Add Sorting Logic Back ---
            query = sortBy?.ToLowerInvariant() switch // Use ToLowerInvariant for case-insensitive matching
            {
                "priceasc" => query.OrderBy(p => p.Price),
                "pricedesc" => query.OrderByDescending(p => p.Price),
                "namedesc" => query.OrderByDescending(p => p.Name),
                // Default to Name Ascending ("nameasc" or anything else/null)
                _ => query.OrderBy(p => p.Name)
            };
            // Log the sort that was actually applied (handle null sortBy for default case)
            Console.WriteLine($"API: Applying sort: {sortBy?.ToLowerInvariant() ?? "nameAsc (default)"}");
            // --- End Sorting Logic ---


            var products = await query.ToListAsync();
            Console.WriteLine($"API: Returning {products.Count} products.");

            return Ok(products);
        }


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