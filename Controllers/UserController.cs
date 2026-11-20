using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Type_It_Out_Backend.Data;
using Type_It_Out_Backend.DTO.Articles;
using Type_It_Out_Backend.DTOs.Users;
using Type_It_Out_Backend.Models;

namespace Type_It_Out_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(AppDbContext context) : Controller
    {

        private readonly AppDbContext _context = context;


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Bio,
                    u.ProfileImageUrl,
                    u.CreatedAt,

                    ArticlesCount = u.Articles.Count(),

                    FollowersCount = _context.Follows
                        .Count(f => f.FollowingId == u.Id),

                    FollowingCount = _context.Follows
                        .Count(f => f.FollowerId == u.Id)
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }


        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateUserProfileDto dto)
        {
            // Identify the logged-in user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var username = dto.Username.Trim();

            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { message = "Username cannot be empty." });

            // Check whether another user already has this username
            var usernameExists = await _context.Users.AnyAsync(u =>
                u.Username == username && u.Id != userId);

            if (usernameExists)
                return Conflict(new { message = "Username is already taken." });

            // Update profile
            user.Username = username;
            user.Bio = dto.Bio?.Trim();
            user.ProfileImageUrl = dto.ProfileImageUrl?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.Id,
                user.Username,
                user.Bio,
                user.ProfileImageUrl
            });
        }


        [Authorize]
        [HttpGet("{id}/articles")]
        public async Task<IActionResult> UserArticles(int id)
        {

            var articles = await _context.Articles.Where(a => a.AuthorId == id).OrderByDescending(a => a.CreatedAt)
                .Select(a => new ArticlesResponseDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    LikesCount = a.Users.Count,
                    CommentsCount = a.Comments.Count,
                    Author = new AuthorDto
                    {
                        Id = a.Author.Id,
                        Username = a.Author.Username,
                        ProfileImageUrl = a.Author.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return Ok(articles);
        }
    }
}
