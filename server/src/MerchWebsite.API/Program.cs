var builder = WebApplication.CreateBuilder(args);

using MerchWebsite.API.Data; // Add this using statement
using Microsoft.EntityFrameworkCore; // Add this using statement

// Add services to the container.

// Configure DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


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
