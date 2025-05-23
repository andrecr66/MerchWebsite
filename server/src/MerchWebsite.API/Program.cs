using MerchWebsite.API.Data;
// --- Remove Duplicate Using ---
// using MerchWebsite.API.Data;
// --- End Remove ---
using MerchWebsite.API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MerchWebsite.API.Services;

// --- Define Seed Method ---
static async Task SeedDatabaseAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>(); // Needed for Users/Roles if seeding those
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>(); // Needed for Roles

            // Apply any pending migrations first
            //await context.Database.MigrateAsync();

            // --- Seed Roles (ensure IDs are static) ---
            if (!await roleManager.Roles.AnyAsync())
            {
                var roles = new List<IdentityRole>
                {
                    new IdentityRole { Name = "Member", NormalizedName = "MEMBER", Id = "c7b013f0-5201-4317-abd8-c211f91b7331", ConcurrencyStamp = "18b4a3d1-f1e4-4f2d-b0f5-c8e0b7a1e5b8" },
                    new IdentityRole { Name = "Admin", NormalizedName = "ADMIN", Id = "c7b013f0-5201-4317-abd8-c211f91b7330", ConcurrencyStamp = "a2b9c4e2-g2f5-5a3e-c1a6-d9f1c8b2f6c9" }
                };
                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
                Console.WriteLine("Seeded roles.");
            }

            // --- Seed Products with Categories ---
            // Inside SeedDatabaseAsync in Program.cs

            // --- Seed Products (Replace previous list) ---
            if (!context.Products.Any())
            {
                var products = new List<Product>
    {
        // T-Shirts (4)
        new Product { Name = "Classic Logo Tee", Description = "Comfortable cotton tee with classic site logo.", Price = 22.50M, ImageUrl = "/assets/images/tee_logo_classic.jpg", Category = "T-Shirts", Gender = "Unisex", AverageRating = null, NumberOfReviews = 0 },
        new Product { Name = "Vintage Wash Tee", Description = "Soft vintage wash effect t-shirt.", Price = 25.00M, ImageUrl = "/assets/images/tee_vintage_wash.jpg", Category = "T-Shirts", Gender = "Men", AverageRating = null, NumberOfReviews = 0 },
        new Product { Name = "Organic Cotton V-Neck", Description = "Sustainable and soft organic cotton v-neck.", Price = 28.00M, ImageUrl = "/assets/images/tee_vneck_organic.jpg", Category = "T-Shirts", Gender = "Women", AverageRating = null, NumberOfReviews = 0 },
        new Product { Name = "Graphic Print Tee", Description = "T-shirt with a unique graphic design.", Price = 24.99M, ImageUrl = "/assets/images/tee_graphic.jpg", Category = "T-Shirts", Gender = "Unisex", AverageRating = null, NumberOfReviews = 0 },
        // Hoodies (2)
        new Product { Name = "Zip-Up Hoodie", Description = "Versatile full zip-up hoodie.", Price = 55.00M, ImageUrl = "/assets/images/hoodie_zipup.jpg", Category = "Hoodies", Gender = "Unisex", AverageRating = null, NumberOfReviews = 0 },
        new Product { Name = "Pullover Hoodie - Heavyweight", Description = "Extra warm heavyweight pullover.", Price = 60.00M, ImageUrl = "/assets/images/hoodie_heavy.jpg", Category = "Hoodies", Gender = "Men", AverageRating = null, NumberOfReviews = 0 },
        // Accessories (4)
        new Product { Name = "Embroidered Baseball Cap", Description = "Classic baseball cap with embroidered logo.", Price = 20.00M, ImageUrl = "/assets/images/cap_baseball.jpg", Category = "Accessories", Gender = "Unisex", AverageRating = null, NumberOfReviews = 0 },
        new Product { Name = "Canvas Tote Bag", Description = "Durable canvas tote bag for everyday use.", Price = 15.50M, ImageUrl = "/assets/images/bag_tote.jpg", Category = "Accessories", Gender = null, AverageRating = null, NumberOfReviews = 0 },
        new Product { Name = "Stainless Steel Water Bottle", Description = "Insulated water bottle, keeps drinks cold/hot.", Price = 29.95M, ImageUrl = "/assets/images/bottle_steel.jpg", Category = "Accessories", Gender = null, AverageRating = null, NumberOfReviews = 0 },
        new Product { Name = "Enamel Pin Set (3 Pins)", Description = "Set of three cool enamel pins.", Price = 12.00M, ImageUrl = "/assets/images/pins_set.jpg", Category = "Accessories", Gender = null, AverageRating = null, NumberOfReviews = 0 },
    };

                // Assign a default SellerId (replace with a real Seller User ID from your AspNetUsers table)
                // Find a user ID first, or hardcode if you know one for testing.
                // Example: Assuming 'userManager' is available and a user 'testseller' exists
                // var sellerUser = await userManager.FindByNameAsync("testseller"); // Or FindByIdAsync
                // string defaultSellerId = sellerUser?.Id ?? "DEFAULT_SELLER_ID_ERROR"; // Handle user not found



                context.Products.AddRange(products);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded 10 sample products.");
            }
            else
            {
                Console.WriteLine("Products table already contains data, skipping product seeding.");
            }

            // Add user seeding here if desired
            // ...

        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during database seeding or migration.");
            // Optional: throw; // Re-throw if you want seeding failure to stop the app
        }
    }
}
// --- End Seed Method Definition ---


// --- Main Application Setup ---
var builder = WebApplication.CreateBuilder(args);

var AllowAngularDevClient = "_allowAngularDevClient";

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options => { /* Identity options */ })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Using AddControllers instead of AddOpenApi for standard API setup
// builder.Services.AddOpenApi(); // Removed - usually AddEndpointsApiExplorer + AddSwaggerGen
builder.Services.AddControllers();
// --- Add Swagger/OpenAPI if desired ---
builder.Services.AddEndpointsApiExplorer(); // Needed for Swagger
builder.Services.AddSwaggerGen(); // Add Swagger generation
// --- End Swagger ---


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAngularDevClient, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddScoped<ITokenService, TokenService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Use Swagger in Development
    app.UseSwagger();
    app.UseSwaggerUI();
    // --- Call Seeding Logic in Development ---
    await SeedDatabaseAsync(app); // Call the seeding method
    // --- End Seeding Call ---
}

// --- Standard Middleware Order ---
app.UseHttpsRedirection(); // Optional: Add HTTPS redirection

app.UseCors(AllowAngularDevClient);

app.UseAuthentication();
app.UseAuthorization(); // Ensure this is added

app.MapControllers();
// --- End Middleware ---

app.Run();