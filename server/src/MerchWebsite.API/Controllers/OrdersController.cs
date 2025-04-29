// ~/Projects/MerchWebsite/server/src/MerchWebsite.API/Controllers/OrdersController.cs
using MerchWebsite.API.Data;
using MerchWebsite.API.Entities;
using MerchWebsite.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer; // For explicit scheme
using System.Linq; // For Linq methods like .Select, .Sum

namespace MerchWebsite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route: /api/orders
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // All order actions require auth
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // --- ADD Method to Get User's Orders ---
        [HttpGet] // GET /api/orders
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersForUser()
        {
            // 1. Get User ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID claim not found.");

            Console.WriteLine($"API: Fetching orders for user {userId}");

            // 2. Query Orders for the user
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items) // Eager load items
                                       // .ThenInclude(i => i.Product) // Usually NOT needed if OrderItem has snapshot data
                .OrderByDescending(o => o.OrderDate) // Show newest orders first
                .Select(o => new OrderDto // Map to DTO
                {
                    Id = o.Id,
                    UserId = o.UserId, // Keep internal if needed, or omit from DTO later
                    OrderDate = o.OrderDate,
                    ShippingAddress_FullName = o.ShippingAddress_FullName, // Include key info for list display
                    ShippingAddress_City = o.ShippingAddress_City, // Example key info
                    ShippingAddress_Country = o.ShippingAddress_Country, // Example key info
                    Subtotal = o.Subtotal,
                    ShippingFee = o.ShippingFee,
                    GrandTotal = o.GrandTotal,
                    // Status = o.Status.ToString(), // If status is implemented
                    // Map OrderItems to OrderItemDtos
                    Items = o.Items.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName, // Use snapshotted data
                        ProductImageUrl = oi.ProductImageUrl, // Use snapshotted data
                        Price = oi.Price, // Use snapshotted data
                        Quantity = oi.Quantity
                    }).ToList()
                })
                .ToListAsync(); // Execute query

            Console.WriteLine($"API: Found {orders.Count} orders for user {userId}.");
            return Ok(orders); // Return 200 OK with the list of OrderDtos
        }
        // --- END Method to Get User's Orders ---
        // POST /api/orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderDto createOrderDto)
        {
            // --- 1. Get User ID ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // Should be handled by [Authorize], but defensive check
                return Unauthorized("User ID claim not found.");
            }

            // --- 2. Get User's Cart (Must include Items and their Products) ---
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product) // Need product details for snapshotting
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest(new ProblemDetails { Title = "Cannot place order: Cart is empty." });
            }

            // --- 3. Create Order Items list from Cart Items ---
            var orderItems = new List<OrderItem>();
            decimal subtotal = 0;
            foreach (var cartItem in cart.Items)
            {
                // Defensive check if product wasn't loaded (shouldn't happen with Include)
                if (cartItem.Product == null)
                {
                    return StatusCode(500, new ProblemDetails { Title = "Error retrieving product details for cart item.", Detail = $"ProductId: {cartItem.ProductId}" });
                }

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name, // Snapshot name
                    ProductImageUrl = cartItem.Product.ImageUrl, // Snapshot image URL
                    Price = cartItem.Product.Price, // Snapshot price per item
                    Quantity = cartItem.Quantity
                    // OrderId/Order navigation property set automatically by EF
                };
                orderItems.Add(orderItem);
                subtotal += orderItem.TotalPrice; // Accumulate subtotal
            }

            // --- 4. Calculate Totals (Example: Fixed shipping fee) ---
            decimal shippingFee = (subtotal > 100) ? 0 : 9.99m; // Free shipping over 00 example
            decimal grandTotal = subtotal + shippingFee;

            // --- 5. Create the Order entity ---
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress_FullName = createOrderDto.ShippingAddress_FullName,
                ShippingAddress_AddressLine1 = createOrderDto.ShippingAddress_AddressLine1,
                ShippingAddress_AddressLine2 = createOrderDto.ShippingAddress_AddressLine2,
                ShippingAddress_City = createOrderDto.ShippingAddress_City,
                ShippingAddress_PostalCode = createOrderDto.ShippingAddress_PostalCode,
                ShippingAddress_Country = createOrderDto.ShippingAddress_Country,
                Subtotal = subtotal,
                ShippingFee = shippingFee,
                GrandTotal = grandTotal,
                Items = orderItems // Assign the list of OrderItems
                // Status defaults if defined in entity
            };

            // --- 6. Add Order to Context ---
            // EF Core change tracking will automatically add the related OrderItems too
            _context.Orders.Add(order);

            // --- 7. Clear the User's Cart ---
            // Remove CartItems first
            _context.CartItems.RemoveRange(cart.Items);
            // Remove Cart itself (or just items depending on desired behavior)
            _context.Carts.Remove(cart);

            // --- 8. Save ALL Changes (Order creation + Cart deletion) ---
            // Consider wrapping in a Transaction if more complex operations are involved later
            var result = await _context.SaveChangesAsync();

            if (result <= 0) // Should be > 0 if order/items added and cart/items removed
            {
                return StatusCode(500, new ProblemDetails { Title = "Failed to save the order." });
            }

            // --- 9. Map created Order to OrderDto and Return ---
            // We need to map manually here as we don't re-query
            var orderDto = new OrderDto
            {
                Id = order.Id, // The ID generated by the database
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                ShippingAddress_FullName = order.ShippingAddress_FullName,
                ShippingAddress_AddressLine1 = order.ShippingAddress_AddressLine1,
                ShippingAddress_AddressLine2 = order.ShippingAddress_AddressLine2,
                ShippingAddress_City = order.ShippingAddress_City,
                ShippingAddress_PostalCode = order.ShippingAddress_PostalCode,
                ShippingAddress_Country = order.ShippingAddress_Country,
                Subtotal = order.Subtotal,
                ShippingFee = order.ShippingFee,
                GrandTotal = order.GrandTotal,
                // Status = order.Status.ToString(), // If using status enum
                Items = order.Items.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductImageUrl = oi.ProductImageUrl,
                    Price = oi.Price,
                    Quantity = oi.Quantity
                }).ToList()
            };

            // Return 201 Created with the location of the new resource (optional but good practice)
            // and the created OrderDto
            // return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderDto); // Need a GetOrderById method for this
            return Ok(orderDto); // Simpler: return 200 OK with the OrderDto for now
        }

        // TODO: Add endpoint to get order details later (e.g., GET /api/orders/{id})
        // [HttpGet("{id:int}", Name = "GetOrderById")]
        // public async Task<ActionResult<OrderDto>> GetOrderById(int id) { ... }
    }
}
