// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Controllers/CartController.cs
using MerchWebsite.API.Data;
using MerchWebsite.API.Entities;
using MerchWebsite.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization; // Ensure this is uncommented
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Ensure this is uncommented
using System.Linq; // Ensure this is uncommented
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MerchWebsite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/cart/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "Cart controller is alive!" });
        }

        // GET /api/cart
        [HttpGet]
        // Specify the scheme explicitly
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // <<<--- MODIFIED
        public async Task<ActionResult<CartDto>> GetCart()
        {
            // --- 1. Get User ID ---
            // Ensure this block is UNCOMMENTED and ACTIVE
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // This can happen if the token is valid but missing the required claim
                return Unauthorized("User ID claim (NameIdentifier) not found in token.");
            }
            // --- End of User ID block ---

            // --- 2. Find or Create Cart ---
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId); // Uses the actual userId

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // --- 3. Map Entity to DTO ---
            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    // Assuming Product is never null due to INNER JOIN in most DBs
                    // or handled by Include/ThenInclude properly loading related data.
                    // Add defensive checks if Product could potentially be null in your data.
                    ProductName = i.Product.Name,
                    ProductImageUrl = i.Product.ImageUrl,
                    Price = i.Product.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            // --- 4. Return DTO ---
            return Ok(cartDto);
        }
    }
}