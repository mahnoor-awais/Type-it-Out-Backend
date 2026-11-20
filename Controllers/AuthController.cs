using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Type_It_Out_Backend.Data;
using Type_It_Out_Backend.DTOs.Auth;
using Type_It_Out_Backend.Models;
using TypeItOutBackend.DTOs.Auth;

namespace Type_It_Out_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AppDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration config) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
        private readonly IConfiguration _config = config;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var username = request.Username.Trim();
            var email = request.Email.Trim();

            var userExists = await _context.Users.AnyAsync(u =>
                u.Username == username || u.Email == email);

            if (userExists)
            {
                return Conflict(new
                {
                    message = "Username or email already exists."
                });
            }

            var user = new User
            {
                Username = username,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password
            );

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new RegisterResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return StatusCode(200, response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var email = request.Email.Trim();
            var password = request.Password.Trim();

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var isPasswordValid = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            ) == PasswordVerificationResult.Success;

            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    message = "Invalid credentials."
                });
            }

            var token = GenerateJwtToken(user);
            LoginResponseDto response = new()
            {
                Token = token,
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
