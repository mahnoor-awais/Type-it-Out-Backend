
using System.ComponentModel.DataAnnotations;

namespace Type_It_Out_Backend.DTOs.Articles;

public class UpdateArticleDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}
