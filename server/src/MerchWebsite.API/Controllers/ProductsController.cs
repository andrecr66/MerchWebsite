using MerchWebsite.API.Data; // Access DbContext
using MerchWebsite.API.Entities; // Access Product entity
using Microsoft.AspNetCore.Mvc; // Use MVC attributes and types
using Microsoft.EntityFrameworkCore; // Use EF Core async methods

namespace MerchWebsite.API.Controllers
{
    [ApiController] // Indicates this is an API controller
    [Route("api/[controller]")] // Sets the base route: api/products
    public class ProductsController : ControllerBase // Base class for API controllers
    {
        private readonly AppDbContext _context;

        // Constructor injection: ASP.NET Core provides the AppDbContext instance
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // Asynchronously retrieve all products from the database
            var products = await _context.Products.ToListAsync();
            return Ok(products); // Return 200 OK with the list of products
        }

        // GET: api/products/5
        [HttpGet("{id}")] // Route parameter for the product ID
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            // Asynchronously find a product by its ID
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(); // Return 404 Not Found if product doesn't exist
            }

            return Ok(product); // Return 200 OK with the found product
        }

        // We will add POST, PUT, DELETE endpoints later
    }
}
