using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Type_It_Out_Backend.Data;
using Type_It_Out_Backend.DTO.Articles;
using Type_It_Out_Backend.DTOs.Articles;
using Type_It_Out_Backend.DTOs.Comments;
using Type_It_Out_Backend.Models;

namespace Type_It_Out_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        [HttpGet("articles")]
        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .OrderByDescending(a => a.CreatedAt)
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

        [Authorize]
        [HttpGet("followingArticles")]
        public async Task<IActionResult> GetFollowingArticles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var articles = await _context.Articles
                .AsNoTracking()
                .Where(a => _context.Follows.Any(f =>
                    f.FollowerId == userId && f.FollowingId == a.AuthorId))
                .OrderByDescending(a => a.CreatedAt)
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

        [HttpGet("articleDetail")]
        public async Task<IActionResult> GetArticleById([FromQuery] int id)
        {
            var article = await _context.Articles
                .Where(a => a.Id == id)
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
                .SingleOrDefaultAsync();
            if (article == null)
                return NotFound();
            return Ok(article);
        }

        [Authorize]
        [HttpPost("articles")]
        public async Task<IActionResult> CreateArticle(ArticlesRequestDto request)
        {
            var newArticle = new Article
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = request.AuthorId
            };

            _context.Articles.Add(newArticle);
            await _context.SaveChangesAsync();

            var author = await _context.Users.FindAsync(newArticle.AuthorId);
            ArticlesResponseDto response = new()
            {
                Id = newArticle.Id,
                Title = newArticle.Title,
                Content = newArticle.Content,
                CreatedAt = newArticle.CreatedAt,
                UpdatedAt = newArticle.UpdatedAt,
                LikesCount = 0,
                CommentsCount = 0,
                Author = new AuthorDto
                {
                    Id = newArticle.AuthorId,
                    Username = author?.Username ?? string.Empty,
                    ProfileImageUrl = author?.ProfileImageUrl
                }
            };
            return CreatedAtAction(nameof(GetArticleById), new { id = newArticle.Id }, response);
        }

        [HttpGet("{id:int}/likes")]
        public async Task<IActionResult> GetArticleLikes(int id)
        {
            var article = await _context.Articles
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    ArticleId = a.Id,

                    Users = a.Users.Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.ProfileImageUrl
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (article == null)
            {
                return NotFound(new
                {
                    message = "Article not found."
                });
            }

            return Ok(new
            {
                articleId = article.ArticleId,
                likesCount = article.Users.Count,
                users = article.Users
            });
        }

        [Authorize]
        [HttpPost("{id:int}/likes")]
        public async Task<IActionResult> LikeArticle(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var article = await _context.Articles
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound(new
                {
                    message = "Article not found."
                });
            }

            var alreadyLiked = article.Users.Any(u => u.Id == userId);

            if (alreadyLiked)
            {
                return Conflict(new
                {
                    message = "You have already liked this article."
                });
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return Unauthorized();

            article.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Article liked successfully.",
                likesCount = article.Users.Count
            });
        }

        [Authorize]
        [HttpDelete("{id:int}/likes")]
        public async Task<IActionResult> UnlikeArticle(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var article = await _context.Articles
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound(new
                {
                    message = "Article not found."
                });
            }

            var user = article.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "You haven't liked this article."
                });
            }

            article.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Article unliked successfully.",
                likesCount = article.Users.Count
            });
        }


        [Authorize]
        [HttpPost("{id:int}/comments")]
        public async Task<IActionResult> AddComment(
            int id,
            CreateCommentDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var articleExists = await _context.Articles
                .AnyAsync(a => a.Id == id);

            if (!articleExists)
            {
                return NotFound(new
                {
                    message = "Article not found."
                });
            }

            var content = request.Content.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(new
                {
                    message = "Comment cannot be empty."
                });
            }

            var comment = new Comment
            {
                ArticleId = id,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                comment.Id,
                comment.ArticleId,
                comment.UserId,
                comment.Content,
                comment.CreatedAt
            });
        }

        [HttpGet("{id:int}/comments")]
        public async Task<IActionResult> GetArticleComments(int id)
        {
            var article = await _context.Articles
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    ArticleId = a.Id,

                    Comments = a.Comments.Select(c => new
                    {
                        c.Id,
                        User = new
                        {
                            c.User.Id,
                            c.User.Username,
                            c.User.ProfileImageUrl
                        },
                        c.Content,
                        c.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (article == null)
            {
                return NotFound(new
                {
                    message = "Article not found."
                });
            }

            return Ok(new
            {
                articleId = article.ArticleId,
                commensCount = article.Comments.Count,
                comments = article.Comments,

            });
        }

        [Authorize]
        [HttpDelete("{articleId:int}/comments/{commentId:int}")]
        public async Task<IActionResult> DeleteComment(int articleId, int commentId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c =>
                    c.Id == commentId &&
                    c.ArticleId == articleId);

            var articleExists = await _context.Articles
                .AnyAsync(a => a.Id == articleId);

            if (!articleExists)
            {
                return NotFound(new
                {
                    message = "Article not found."
                });
            }

            if (comment == null)
            {
                return NotFound(new
                {
                    message = "Comment not found."
                });
            }

            if (comment.UserId != userId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment deleted successfully."
            });
        }


        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateArticle(int id, UpdateArticleDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
                return NotFound(new { message = "Article not found." });

            if (article.AuthorId != userId)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Title) ||
                string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new
                {
                    message = "Title and content cannot be empty."
                });
            }

            article.Title = dto.Title.Trim();
            article.Content = dto.Content.Trim();
            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Article updated successfully.",
                article.Id,
                article.Title,
                article.Content,
                article.UpdatedAt
            });
        }


        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var article = await _context.Articles.Include(a => a.Users).FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
                return NotFound(new { message = "Article not found." });

            if (article.AuthorId != userId)
                return Forbid();

            article.Users.Clear();
            var comments = await _context.Comments
                .Where(c => c.ArticleId == id)
                .ToListAsync();
            _context.Comments.RemoveRange(comments);
            _context.Articles.Remove(article);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Article deleted successfully."
            });
        }


    }
}
