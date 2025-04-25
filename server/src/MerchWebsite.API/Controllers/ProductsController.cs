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
        [FromQuery] string? sortBy,
        [FromQuery] string? gender,     // <<< Add gender param
        [FromQuery] decimal? minPrice, // <<< Add minPrice param
        [FromQuery] decimal? maxPrice) // <<< Add maxPrice param
        {
            // --- Update Logging ---
            Console.WriteLine($"API: Request QueryString: {Request.QueryString}");
            foreach (var item in Request.Query) { Console.WriteLine($"API: Query Param: {item.Key} = {item.Value}"); }
            Console.WriteLine($"API: Request received (parameters). Category: '{category}', SortBy: '{sortBy}', Gender: '{gender}', MinPrice: {minPrice}, MaxPrice: {maxPrice}");
            // --- End Update Logging ---

            var query = _context.Products.AsQueryable();

            // --- Apply Filters ---
            if (!string.IsNullOrEmpty(category))
            {
                Console.WriteLine($"API: Applying category filter: {category}");
                query = query.Where(p => EF.Functions.ILike(p.Category, category));
            }

            // --- ADD Gender Filter ---
            if (!string.IsNullOrEmpty(gender))
            {
                Console.WriteLine($"API: Applying gender filter: {gender}");
                // Assumes Gender stores values like "Men", "Women", "Unisex" etc.
                // Use ILike for case-insensitivity. Handle null Gender property on products.
                query = query.Where(p => p.Gender != null && EF.Functions.ILike(p.Gender, gender));
            }
            // --- END Gender Filter ---

            // --- ADD Price Filters ---
            if (minPrice.HasValue)
            {
                Console.WriteLine($"API: Applying minPrice filter: {minPrice.Value}");
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                Console.WriteLine($"API: Applying maxPrice filter: {maxPrice.Value}");
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            // --- END Price Filters ---
            // --- End Apply Filters ---


            // --- Sorting Logic (Keep existing) ---
            query = sortBy?.ToLowerInvariant() switch
            {
                "priceasc" => query.OrderBy(p => p.Price),
                "pricedesc" => query.OrderByDescending(p => p.Price),
                "namedesc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            };
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