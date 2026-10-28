using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Type_It_Out_Backend.Models;

[Index("Username", Name = "UQ__Users__536C85E42D8F865F", IsUnique = true)]
[Index("Email", Name = "UQ__Users__A9D10534D830EA26", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    [InverseProperty("User")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Follower")]
    public virtual ICollection<Follow> FollowFollowers { get; set; } = new List<Follow>();

    [InverseProperty("Following")]
    public virtual ICollection<Follow> FollowFollowings { get; set; } = new List<Follow>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Article> ArticlesNavigation { get; set; } = new List<Article>();
}
