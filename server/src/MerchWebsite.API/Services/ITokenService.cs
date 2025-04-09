using MerchWebsite.API.Entities;

namespace MerchWebsite.API.Services
{
    public interface ITokenService
    {
        // Method signature to create a token for a given user
        string CreateToken(User user);
    }
}
