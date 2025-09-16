using System.Security.Claims;

namespace TransitAFC.Shared.Security
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, string firstName, string lastName, List<string>? roles = null);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool ValidateToken(string token);
        Guid? GetUserIdFromToken(string token);
        string? GetEmailFromToken(string token);
        DateTime GetTokenExpiry(string token);
    }

    
}