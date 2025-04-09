using MerchWebsite.API.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MerchWebsite.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string? _issuer;
        private readonly string? _audience;

        // Inject IConfiguration to read settings from appsettings/user secrets
        public TokenService(IConfiguration config)
        {
            // Retrieve the secret key from configuration (User Secrets)
            var keyString = config["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new ArgumentNullException(nameof(keyString), "JWT Key is not configured in User Secrets or appsettings.");
            }
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            // Retrieve Issuer and Audience
            _issuer = config["Jwt:Issuer"];
            _audience = config["Jwt:Audience"];

            if (string.IsNullOrEmpty(_issuer) || string.IsNullOrEmpty(_audience))
            {
                throw new ArgumentNullException(nameof(config), "JWT Issuer or Audience is not configured.");
            }
        }

        public string CreateToken(User user)
        {
            // Define claims for the token (information about the user)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id), // Standard claim for user ID
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? "Unknown"), // Standard claim for username
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "Unknown"), // Standard claim for email
                // Add other claims as needed (e.g., roles)
            };

            // Create signing credentials using the symmetric key and HMAC SHA512 algorithm
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // Describe the token (claims, expiration, signing credentials, issuer, audience)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiry (e.g., 7 days) - adjust as needed
                SigningCredentials = creds,
                Issuer = _issuer,
                Audience = _audience
            };

            // Create and write the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token); // Return the serialized token string
        }
    }
}
