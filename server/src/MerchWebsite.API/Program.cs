using MerchWebsite.API.Data;
using MerchWebsite.API.Data;
using MerchWebsite.API.Entities; // Add this for User entity
using Microsoft.AspNetCore.Authentication.JwtBearer; // Add this for JWT Bearer
using Microsoft.AspNetCore.Identity; // Add this for Identity services
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Add this for Token Validation
using System.Text; // Add this for Encoding
using MerchWebsite.API.Services; // Add this for TokenService

var builder = WebApplication.CreateBuilder(args);

var AllowAngularDevClient = "_allowAngularDevClient"; // Define policy name

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

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowAngularDevClient,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200") // Allow Angular dev server origin
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Add Authentication services and configure JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Validate the server that generated the token
            ValidateAudience = true, // Validate the recipient of the token is authorized to receive it
            ValidateLifetime = true, // Check if the token is expired
            ValidateIssuerSigningKey = true, // Validate the signature of the token
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Get Issuer from config (User Secrets)
            ValidAudience = builder.Configuration["Jwt:Audience"], // Get Audience from config (User Secrets)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)) // Get Key from config (User Secrets)
        };
    });

// Register custom services
builder.Services.AddScoped<ITokenService, TokenService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Apply CORS middleware - IMPORTANT: Must be before Authentication/Authorization and MapControllers
app.UseCors(AllowAngularDevClient);

// Add Authentication middleware - IMPORTANT: Must be before Authorization (if added later) and MapControllers
app.UseAuthentication();
// app.UseAuthorization(); // We'll add Authorization later if needed for specific endpoints

// Map controller routes
app.MapControllers(); // Add this to map API controller routes

app.Run();

// Removed WeatherForecast related code
