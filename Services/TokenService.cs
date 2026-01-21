using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DocumentManager.Api.Models;

namespace DocumentManager.Api.Services
{
    public class TokenService
    {
        private readonly string _jwtKey;

        public TokenService(IConfiguration configuration)
        {
            _jwtKey = configuration["Jwt:Key"]
                ?? throw new ArgumentNullException("JWT Key not configured in appsettings.json");
        }

        /// <summary>
        /// Генерация JWT токена для пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="expiresInHours">Время жизни токена в часах</param>
        /// <returns>JWT token string</returns>
        public string GenerateToken(User user, int expiresInHours = 2)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                // Добавляем роли при необходимости
                // new Claim(ClaimTypes.Role, "Admin")
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
}
