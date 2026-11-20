using System.ComponentModel.DataAnnotations;

namespace Type_It_Out_Backend.DTO.Articles;

/// <summary>Request to like an article as the authenticated user.</summary>
public sealed record CreateArticleLikeRequestDto
{
    [Range(1, int.MaxValue)]
    public int ArticleId { get; init; }

    [Range(1, int.MaxValue)]
    public int UserId { get; init; }
}
