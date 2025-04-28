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
            if (!context.Products.Any())
            {
                // Inside SeedDatabaseAsync in Program.cs, within the product list creation

                // Example snippet from SeedDatabaseAsync in Program.cs
                // Inside SeedDatabaseAsync in Program.cs
                // Corrected example snippet within SeedDatabaseAsync
                var products = new List<Product>
            {
                new Product {
                    Id = 0, // Let DB generate ID
                    Name = "Awesome T-Shirt",
                    Description = "High-quality cotton t-shirt with awesome design.", // <<< Ensure present
                    Price = 19.99M, // <<< Ensure present and non-zero with M
                    ImageUrl = "/assets/images/tshirt_awesome.jpg", // <<< Ensure present
                    Category = "T-Shirts", // <<< Ensure present
                    Gender = "Unisex", // <<< Ensure present
                    AverageRating = 4.5,
                    NumberOfReviews = 120
                },
                new Product {
                    Id = 0,
                    Name = "Cool Hoodie",
                    Description = "Warm and comfortable hoodie.", // <<< Ensure present
                    Price = 49.99M, // <<< Ensure present and non-zero with M
                    ImageUrl = "/assets/images/hoodie_cool.jpg", // <<< Ensure present
                    Category = "Hoodies", // <<< Ensure present
                    Gender = "Unisex", // <<< Ensure present
                    AverageRating = 4.8,
                    NumberOfReviews = 85
                },
                new Product {
                    Id = 0,
                    Name = "Stylish Mug",
                    Description = "Ceramic mug, perfect for your morning coffee.", // <<< Ensure present
                    Price = 9.99M, // <<< Ensure present and non-zero with M
                    ImageUrl = "/assets/images/mug_stylish.jpg", // <<< Ensure present
                    Category = "Accessories", // <<< Ensure present
                    Gender = null, // <<< Gender can be null if intended
                    AverageRating = 4.2,
                    NumberOfReviews = 55
                },
                new Product {
                    Id = 0,
                    Name = "Basic Black T-Shirt",
                    Description = "Simple, essential black t-shirt.", // <<< Ensure present
                    Price = 14.99M, // <<< Ensure present and non-zero with M
                    ImageUrl = "/assets/images/tshirt_black.jpg", // <<< Ensure present
                    Category = "T-Shirts", // <<< Ensure present
                    Gender = "Men", // <<< Ensure present
                    AverageRating = 3.9,
                    NumberOfReviews = 210
                },
                new Product {
                    Id = 0,
                    Name = "Fitted V-Neck T-Shirt",
                    Description = "Soft, fitted v-neck.", // <<< Ensure present
                    Price = 18.99M, // <<< Ensure present and non-zero with M
                    ImageUrl = "/assets/images/tshirt_vneck_women.jpg", // <<< Ensure present
                    Category = "T-Shirts", // <<< Ensure present
                    Gender = "Women", // <<< Ensure present
                    AverageRating = null, // Rating can be null
                    NumberOfReviews = 0
                },
            };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded sample products with categories.");
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