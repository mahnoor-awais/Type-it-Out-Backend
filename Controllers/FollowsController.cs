using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Type_It_Out_Backend.Data;
using Type_It_Out_Backend.Models;

namespace Type_It_Out_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FollowsController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [Authorize]
    [HttpPost("{followingId:int}")]
    public async Task<IActionResult> FollowUser(int followingId)
    {
        // Get the logged-in user's ID from the JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdClaim, out var followerId))
            return Unauthorized();

        // Prevent users from following themselves
        if (followerId == followingId)
            return BadRequest(new { message = "You cannot follow yourself." });

        // Check whether the target user exists
        var targetUser = await _context.Users.FindAsync(followingId);

        if (targetUser == null)
            return NotFound(new { message = "User not found." });

        // Check whether the follow already exists
        var existingFollow = await _context.Follows
            .AnyAsync(f => f.FollowerId == followerId &&
                           f.FollowingId == followingId);

        if (existingFollow)
            return Conflict(new { message = "You are already following this user." });

        // Create the follow relationship
        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User followed successfully." });
    }

    [Authorize]
    [HttpDelete("{followingId:int}")]
    public async Task<IActionResult> UnfollowUser(int followingId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdClaim, out var followerId))
            return Unauthorized();

        // Find the follow relationship
        var follow = await _context.Follows.FirstOrDefaultAsync(
            f => f.FollowerId == followerId &&
                 f.FollowingId == followingId
        );

        if (follow == null)
            return NotFound(new { message = "You are not following this user." });

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User unfollowed successfully." });
    }
}
