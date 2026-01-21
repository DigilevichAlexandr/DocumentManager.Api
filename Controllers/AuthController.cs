using DocumentManager.Api.Data;
using DocumentManager.Api.DTOs;
using DocumentManager.Api.Models;
using DocumentManager.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;

namespace DocumentManager.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        //private readonly JwtService _jwt;
        private readonly TokenService _jwt;

        public AuthController(AppDbContext db, TokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (_db.Users.Any(u => u.Email == dto.Email))
                return BadRequest("User already exists");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = dto.Password//BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null ||
                user.PasswordHash != dto.Password)
                //!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized();

            var token = _jwt.GenerateToken(user);
            return Ok(new { token });
        }
    }

}
