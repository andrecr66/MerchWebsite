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

        // Add this method inside the CartController class, below GetCart()

        // POST /api/cart/items
        [HttpPost("items")] // Route: /api/cart/items
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requires authentication
        public async Task<ActionResult<CartDto>> AddItem([FromBody] AddCartItemDto addItemDto)
        {
            // --- 1. Get User ID ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID claim (NameIdentifier) not found in token.");
            }

            // --- 2. Validate Product Exists ---
            var product = await _context.Products.FindAsync(addItemDto.ProductId);
            if (product == null)
            {
                // Use NotFound() which returns 404 - more appropriate than BadRequest here
                return NotFound(new ProblemDetails { Title = "Product not found." });
            }

            // --- 3. Get User's Cart (or create if none) ---
            // Include items to check if product already exists in cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                // No need to save yet, will save later
            }

            // --- 4. Check if Item Already Exists in Cart ---
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == addItemDto.ProductId);

            if (existingItem != null)
            {
                // If item exists, increase quantity
                existingItem.Quantity += addItemDto.Quantity;
            }
            else
            {
                // If item doesn't exist, create a new CartItem
                var newItem = new CartItem
                {
                    CartId = cart.Id, // Will be 0 if cart is new, EF handles relationship fix-up
                    Cart = cart,      // Associate with the cart object
                    ProductId = addItemDto.ProductId,
                    Quantity = addItemDto.Quantity
                    // Product navigation property is implicitly handled by ProductId
                };
                // No need to add to context directly if adding to tracked cart's collection
                cart.Items.Add(newItem); // Add to the cart's collection
                                         // _context.CartItems.Add(newItem); // Alternatively, add directly to context
            }

            // --- 5. Save Changes to Database ---
            var result = await _context.SaveChangesAsync();

            // --- 6. Check if Save Failed ---
            if (result <= 0)
            {
                // Or return a more specific error DTO
                return BadRequest(new ProblemDetails { Title = "Problem saving item to cart." });
            }

            // --- 7. Return Updated Cart DTO ---
            // Fetch the updated cart again to ensure all data (including product details) is fresh
            // This avoids complex manual updates to the DTO after adding/updating
            // Call the existing GetCart logic (refactor potential later)
            // Note: Calling GetCart() directly isn't ideal practice, better to have a shared private method
            // or just requery here for simplicity now.

            // Re-Query to get updated state including Product details for the DTO
            var updatedCart = await _context.Carts
               .Include(c => c.Items)
                   .ThenInclude(i => i.Product)
               .FirstOrDefaultAsync(c => c.UserId == userId);

            if (updatedCart == null) // Should not happen after save, but defensive check
            {
                return StatusCode(500, new ProblemDetails { Title = "Failed to retrieve updated cart." });
            }

            // Map to DTO
            var cartDto = new CartDto
            {
                Id = updatedCart.Id,
                UserId = updatedCart.UserId,
                Items = updatedCart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "N/A",
                    ProductImageUrl = i.Product?.ImageUrl,
                    Price = i.Product?.Price ?? 0,
                    Quantity = i.Quantity
                }).ToList()
            };

            // Return 200 OK with the updated cart state
            return Ok(cartDto);
        }

        // Add this method inside the CartController class

        // DELETE /api/cart/items/{itemId}
        [HttpDelete("items/{itemId:int}")] // Route: /api/cart/items/{itemId}, constraint itemId must be integer
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CartDto>> RemoveItem(int itemId)
        {
            // --- 1. Get User ID ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID claim (NameIdentifier) not found in token.");
            }

            // --- 2. Find User's Cart ---
            // We need the cart primarily to ensure the item belongs to the user
            var cart = await _context.Carts
                                .Include(c => c.Items) // Include items to find the specific one
                                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // If user has no cart, they certainly don't have this item in a cart
                return NotFound(new ProblemDetails { Title = "Cart not found." });
            }

            // --- 3. Find the Specific Item within the User's Cart ---
            var itemToRemove = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (itemToRemove == null)
            {
                // Item ID doesn't exist, or doesn't belong to this user's cart
                return NotFound(new ProblemDetails { Title = "Item not found in cart." });
            }

            // --- 4. Remove the Item ---
            _context.CartItems.Remove(itemToRemove);
            // Alternatively, if tracking is reliable: cart.Items.Remove(itemToRemove);

            // --- 5. Save Changes ---
            var result = await _context.SaveChangesAsync();

            if (result <= 0)
            {
                return BadRequest(new ProblemDetails { Title = "Problem removing item from cart." });
            }

            // --- 6. Return Updated Cart DTO ---
            // Re-Query to get updated state including Product details for the DTO
            var updatedCart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId); // Re-fetch the user's cart

            if (updatedCart == null) // User should still have a cart, even if empty now
            {
                // This indicates a potential issue, maybe cart was deleted concurrently?
                return StatusCode(500, new ProblemDetails { Title = "Failed to retrieve updated cart after removal." });
            }

            // Map to DTO
            var cartDto = new CartDto
            {
                Id = updatedCart.Id,
                UserId = updatedCart.UserId,
                Items = updatedCart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "N/A",
                    ProductImageUrl = i.Product?.ImageUrl,
                    Price = i.Product?.Price ?? 0,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(cartDto);
        }


        // Add this method inside the CartController class

        // PUT /api/cart/items/{itemId}
        [HttpPut("items/{itemId:int}")] // Route: PUT /api/cart/items/{itemId}
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CartDto>> UpdateItemQuantity(int itemId, [FromBody] UpdateCartItemQuantityDto updateDto)
        {
            // --- 1. Get User ID ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID claim (NameIdentifier) not found in token.");
            }

            // --- 2. Find User's Cart ---
            var cart = await _context.Carts
                                .Include(c => c.Items) // Include items to find the specific one
                                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new ProblemDetails { Title = "Cart not found." });
            }

            // --- 3. Find the Specific Item within the User's Cart ---
            var itemToUpdate = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (itemToUpdate == null)
            {
                // Item ID doesn't exist, or doesn't belong to this user's cart
                return NotFound(new ProblemDetails { Title = "Item not found in cart." });
            }

            // --- 4. Update the Quantity ---
            // Validation for Quantity >= 1 is handled by the [Range] attribute on the DTO
            itemToUpdate.Quantity = updateDto.Quantity;

            // --- 5. Save Changes ---
            var result = await _context.SaveChangesAsync();

            if (result <= 0)
            {
                // Note: If quantity didn't actually change, SaveChangesAsync might return 0.
                // Consider if this is truly an error, or just return the current state.
                // For now, we treat 0 changes as potentially problematic.
                return BadRequest(new ProblemDetails { Title = "Problem updating item quantity." });
            }

            // --- 6. Return Updated Cart DTO ---
            // Re-Query to get updated state
            var updatedCart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (updatedCart == null)
            {
                return StatusCode(500, new ProblemDetails { Title = "Failed to retrieve updated cart after update." });
            }

            // Map to DTO
            var cartDto = new CartDto
            {
                Id = updatedCart.Id,
                UserId = updatedCart.UserId,
                Items = updatedCart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "N/A",
                    ProductImageUrl = i.Product?.ImageUrl,
                    Price = i.Product?.Price ?? 0,
                    Quantity = i.Quantity // Use the updated quantity
                }).ToList()
            };

            return Ok(cartDto);
        }
        // Add using System.Linq; if not already present
    }
}