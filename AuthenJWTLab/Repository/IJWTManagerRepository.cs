using AuthenJWTLab.Models;
using System.Security.Claims;

namespace AuthenJWTLab.Repository
{
    public interface IJWTManagerRepository
    {
        Task<Token> GenerateJWTTokens(User user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool IsRefreshTokenValidate(string refreshToken);
    }
}