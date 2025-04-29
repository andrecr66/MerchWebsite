// server/src/MerchWebsite.API/Controllers/ProductsController.cs
using MerchWebsite.API.Data;
using MerchWebsite.API.Entities;
using MerchWebsite.API.Models.DTOs; // <<< Add DTO namespace
using Microsoft.AspNetCore.Authorization; // <<< Add Authorization namespace
using Microsoft.AspNetCore.Identity; // <<< Add Identity namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims; // <<< Add Claims namespace
using Microsoft.AspNetCore.Authentication.JwtBearer; // <<< Add JWT Scheme
using MerchWebsite.API.Models.DTOs; // <<< ADD THIS LINE

namespace MerchWebsite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/products
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager; // <<< Inject UserManager

        public ProductsController(AppDbContext context, UserManager<User> userManager) // <<< Update Constructor
        {
            _context = context;
            _userManager = userManager; // <<< Assign UserManager
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
                // --- ADD Rating Sort ---
                // Sort by rating descending, put products with no rating (null) last
                "ratingdesc" => query.OrderByDescending(p => p.AverageRating ?? -1), // Treat nulls as lowest (-1)
                // --- END Rating Sort ---
                // Default ("nameasc", "relevance", null, or anything else)
                _ => query.OrderBy(p => p.Name)
            };
            string appliedSort = sortBy?.ToLowerInvariant() switch
            {
                "priceasc" => "priceAsc",
                "pricedesc" => "priceDesc",
                "namedesc" => "nameDesc",
                "ratingdesc" => "ratingDesc",
                _ => "nameAsc (default)"
            };
            Console.WriteLine($"API: Applying sort: {appliedSort}");

            var products = await query.ToListAsync();
            Console.WriteLine($"API: Returning {products.Count} products.");

            return Ok(products);
        }


        // --- ADD POST Method for Reviews ---
        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            // ... (existing GetProduct code remains the same) ...
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost("{productId:int}/reviews")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Require login
        public async Task<ActionResult> AddReview(int productId, [FromBody] CreateReviewDto reviewDto)
        {
            // 1. Get User ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID claim not found."); // Should not happen if authorized

            // 2. Check if Product exists
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists) return NotFound($"Product with ID {productId} not found.");

            // 3. (Optional) Check if user already reviewed this product
            var existingReview = await _context.Reviews
                                       .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existingReview != null)
            {
                // Option 1: Update existing review
                existingReview.Rating = reviewDto.Rating;
                existingReview.Comment = reviewDto.Comment;
                existingReview.ReviewDate = DateTime.UtcNow;
                Console.WriteLine($"API: Updating review for product {productId} by user {userId}");
                // _context.Reviews.Update(existingReview); // Update is often tracked automatically

                // Option 2: Prevent multiple reviews (uncomment to enable)
                // return BadRequest(new ProblemDetails { Title = "You have already reviewed this product." });
            }
            else
            {
                // 4. Create new Review Entity if none exists
                var review = new Review
                {
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = DateTime.UtcNow,
                    ProductId = productId,
                    UserId = userId
                    // Product and User navigation properties will be handled by EF if needed
                };
                Console.WriteLine($"API: Adding new review for product {productId} by user {userId}");
                _context.Reviews.Add(review);
            }


            // 5. Save review changes
            var reviewSaveResult = await _context.SaveChangesAsync();
            if (reviewSaveResult <= 0 && existingReview == null) // Check if save failed for NEW review
            {
                return StatusCode(500, new ProblemDetails { Title = "Failed to save review." });
            }

            // 6. Recalculate and Update Product Average Rating & Count
            // Use a separate transaction or ensure atomicity if critical
            var productToUpdate = await _context.Products.FindAsync(productId);
            if (productToUpdate != null)
            {
                var reviewsForProduct = await _context.Reviews
                                                .Where(r => r.ProductId == productId)
                                                .ToListAsync();

                productToUpdate.NumberOfReviews = reviewsForProduct.Count;
                productToUpdate.AverageRating = (reviewsForProduct.Count > 0)
                                                ? reviewsForProduct.Average(r => r.Rating)
                                                : null; // Set to null if no reviews left (e.g., if deletion was implemented)

                Console.WriteLine($"API: Updating product {productId} rating to {productToUpdate.AverageRating}, count {productToUpdate.NumberOfReviews}");
                await _context.SaveChangesAsync(); // Save product update
            }

            // Return success (No Content is common for updates, OK or Created for new)
            return (existingReview != null) ? Ok() : CreatedAtAction(nameof(GetProduct), new { id = productId }, null); // Return 200 OK if updated, 201 Created if new
        }

        // --- ADD Endpoint to GET Reviews ---
        // GET: api/products/{productId}/reviews
        [HttpGet("{productId:int}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsForProduct(int productId)
        {
            Console.WriteLine($"API: Getting reviews for product ID: {productId}"); // Log request

            // Optional: Check if product exists first
            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Query reviews for the product
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User) // <<< Include User data to get UserName
                .OrderByDescending(r => r.ReviewDate) // <<< Show newest reviews first
                .Select(r => new ReviewDto // <<< Map to DTO
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    // Safely access UserName, provide default if User is somehow null
                    UserName = r.User != null ? r.User.UserName ?? "User Unknown" : "User Not Found"
                })
                .ToListAsync(); // Execute query

            Console.WriteLine($"API: Found {reviews.Count} reviews for product {productId}."); // Log count
            return Ok(reviews); // Return 200 OK with the list of review DTOs
        }
        // --- END Endpoint to GET Reviews ---
    }
}