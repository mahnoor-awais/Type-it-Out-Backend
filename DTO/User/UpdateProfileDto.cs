
using System.ComponentModel.DataAnnotations;

namespace Type_It_Out_Backend.DTOs.Users;

public class UpdateUserProfileDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Bio { get; set; }

    [Url]
    public string? ProfileImageUrl { get; set; }
}
