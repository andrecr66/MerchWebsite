using MerchWebsite.API.Data;
using MerchWebsite.API.Entities; // Add this for User entity
using Microsoft.AspNetCore.Identity; // Add this for Identity services
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Identity services
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Configure Identity options here if needed (e.g., password requirements)
    // options.Password.RequireDigit = true;
    // options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); // Add token providers for features like password reset


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add Controllers service (needed for API controllers we will add later)
builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map controller routes
app.MapControllers(); // Add this to map API controller routes

app.Run();

// Removed WeatherForecast related code
