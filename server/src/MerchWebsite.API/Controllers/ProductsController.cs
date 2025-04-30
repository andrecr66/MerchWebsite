// server/src/MerchWebsite.API/Controllers/ProductsController.cs
using MerchWebsite.API.Data;
using MerchWebsite.API.Entities;
using MerchWebsite.API.Models.DTOs; // Ensure this is present for DTOs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Ensure this is present

namespace MerchWebsite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/products
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProductsController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/products?category=SomeCategory&sortBy=priceAsc&...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            [FromQuery] string? category,
            [FromQuery] string? sortBy,
            [FromQuery] string? gender,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            // --- Diagnostic Logging ---
            Console.WriteLine($"API: Request QueryString: {Request.QueryString}");
            foreach (var item in Request.Query) { Console.WriteLine($"API: Query Param: {item.Key} = {item.Value}"); }
            Console.WriteLine($"API: Request received (parameters). Category: '{category}', SortBy: '{sortBy}', Gender: '{gender}', MinPrice: {minPrice}, MaxPrice: {maxPrice}");
            // --- End Diagnostic Logging ---

            var query = _context.Products.AsQueryable();

            // --- Apply Filters ---
            if (!string.IsNullOrEmpty(category))
            {
                Console.WriteLine($"API: Applying category filter: {category}");
                query = query.Where(p => EF.Functions.ILike(p.Category, category));
            }
            if (!string.IsNullOrEmpty(gender))
            {
                Console.WriteLine($"API: Applying gender filter: {gender}");
                query = query.Where(p => p.Gender != null && EF.Functions.ILike(p.Gender, gender));
            }
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
            // --- End Apply Filters ---

            // --- Sorting Logic ---
            query = sortBy?.ToLowerInvariant() switch
            {
                "priceasc" => query.OrderBy(p => p.Price),
                "pricedesc" => query.OrderByDescending(p => p.Price),
                "namedesc" => query.OrderByDescending(p => p.Name),
                "ratingdesc" => query.OrderByDescending(p => p.AverageRating ?? -1),
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
            // --- End Sorting Logic ---

            var products = await query.ToListAsync();
            Console.WriteLine($"API: Returning {products.Count} products.");
            return Ok(products);
        }


        // GET: api/products/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST: api/products/{productId}/reviews
        [HttpPost("{productId:int}/reviews")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> AddReview(int productId, [FromBody] CreateReviewDto reviewDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID claim not found.");

            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists) return NotFound($"Product with ID {productId} not found.");

            var existingReview = await _context.Reviews
                                       .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existingReview != null)
            {
                // Prevent multiple reviews
                return BadRequest(new ProblemDetails { Title = "You have already reviewed this product." });
            }
            else
            {
                // Create new Review
                var review = new Review
                {
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = DateTime.UtcNow,
                    ProductId = productId,
                    UserId = userId
                };
                Console.WriteLine($"API: Adding new review for product {productId} by user {userId}");
                _context.Reviews.Add(review);
            }

            // Save review changes
            var reviewSaveResult = await _context.SaveChangesAsync();
            if (existingReview == null && reviewSaveResult <= 0)
            {
                return StatusCode(500, new ProblemDetails { Title = "Failed to save new review." });
            }

            // --- Recalculate and Update Product ---
            // Use a separate try-catch for this update operation
            try
            {
                var productToUpdate = await _context.Products.FindAsync(productId);
                if (productToUpdate != null)
                {
                    var reviewsForProduct = await _context.Reviews
                                                    .Where(r => r.ProductId == productId)
                                                    .ToListAsync(); // Get all reviews again

                    // Calculate new values
                    productToUpdate.NumberOfReviews = reviewsForProduct.Count;
                    productToUpdate.AverageRating = (reviewsForProduct.Count > 0)
                                                    ? Math.Round(reviewsForProduct.Average(r => r.Rating), 2) // Calculate Average and round
                                                    : null;

                    Console.WriteLine($"API: Calculated Product Update -> Rating: {productToUpdate.AverageRating}, Count: {productToUpdate.NumberOfReviews}");

                    // Check if EF Core detects changes BEFORE saving
                    var changesDetected = _context.ChangeTracker.HasChanges();
                    Console.WriteLine($"API: ChangeTracker HasChanges BEFORE product update save: {changesDetected}"); // <<< ADDED LOG

                    // Explicitly mark the product as modified if needed (sometimes helps)
                    // _context.Entry(productToUpdate).State = EntityState.Modified; // <<< Optional

                    var productSaveResult = await _context.SaveChangesAsync(); // Save product update
                    Console.WriteLine($"API: Product rating/count update SaveChangesAsync result: {productSaveResult}"); // <<< ADDED LOG
                }
                else
                {
                    Console.WriteLine($"API WARNING: Product {productId} not found during rating update attempt.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API: ERROR saving product rating/count update: {ex.Message}");
                // Log the error, but maybe don't fail the whole request as review was saved
                // Consider how critical this update is vs. maybe a background job
            }
            // --- End Recalculate ---

            // Return 201 Created for the new review
            return CreatedAtAction(nameof(GetProduct), new { id = productId }, null);
        }

        // GET: api/products/{productId}/reviews
        [HttpGet("{productId:int}/reviews")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsForProduct(int productId)
        {
            Console.WriteLine($"API: Getting reviews for product ID: {productId}");

            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists) return NotFound($"Product with ID {productId} not found.");

            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate,
                    UserName = r.User != null ? r.User.UserName ?? "User_Error" : "User_Not_Found" // Adjusted default
                })
                .ToListAsync();

            Console.WriteLine($"API: Found {reviews.Count} reviews for product {productId}.");
            return Ok(reviews);
        }
    }
}