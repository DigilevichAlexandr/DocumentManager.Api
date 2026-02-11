using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocumentManager.Api.Application.Interfaces;
using DocumentManager.Api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DocumentManager.Api.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly string _jwtKey;

    public TokenService(IConfiguration configuration)
    {
        _jwtKey = configuration["Jwt:Key"]
            ?? throw new ArgumentNullException("JWT Key not configured in appsettings.json");
    }

    public string GenerateToken(User user, int expiresInHours = 2)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresInHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
