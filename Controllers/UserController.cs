using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Type_It_Out_Backend.Data;
using Type_It_Out_Backend.DTO.Articles;

namespace Type_It_Out_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

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


        [HttpGet("{id:int}/followers")]
        public async Task<IActionResult> GetFollowers(int id)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == id);

            if (!userExists)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var followers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => new
                {
                    f.Follower.Id,
                    f.Follower.Username,
                    f.Follower.ProfileImageUrl
                })
                .ToListAsync();

            return Ok(new
            {
                userId = id,
                followersCount = followers.Count,
                followers
            });
        }


        [HttpGet("{id:int}/following")]
        public async Task<IActionResult> GetFollowing(int id)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == id);

            if (!userExists)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var following = await _context.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => new
                {
                    f.Following.Id,
                    f.Following.Username,
                    f.Following.ProfileImageUrl
                })
                .ToListAsync();

            return Ok(new
            {
                userId = id,
                followingCount = following.Count,
                following
            });
        }


    }
}
