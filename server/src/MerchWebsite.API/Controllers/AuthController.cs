using MerchWebsite.API.Entities;
using MerchWebsite.API.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MerchWebsite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route: api/auth
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        // We'll add a token service later for JWT generation

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null) return Unauthorized("Invalid username or password"); // User not found

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false); // Check password

            if (!result.Succeeded) return Unauthorized("Invalid username or password"); // Password incorrect

            // TODO: Generate and return a JWT token instead of just OK
            return Ok(new { Message = "Login successful" });
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            // Check if username already exists
            var existingUserByUsername = await _userManager.FindByNameAsync(registerDto.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest(new { Message = "Username already exists" });
            }

            // Check if email already exists
            var existingUserByEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new { Message = "Email already exists" });
            }

            // Use fully qualified name for User entity
            var user = new MerchWebsite.API.Entities.User
            {
                UserName = registerDto.Username,
                Email = registerDto.Email
                // Assign other properties from DTO if needed
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                // Log errors or return specific error messages
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem(); // Returns 400 Bad Request with validation errors
            }

            // TODO: Add user to a default role if needed
            // await _userManager.AddToRoleAsync(user, "Member");

            // TODO: Generate and return a JWT token or user info instead of just OK
            return Ok(new { Message = "Registration successful" });
        }
    }
}
