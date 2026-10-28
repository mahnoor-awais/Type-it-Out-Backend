using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Type_It_Out_Backend.Models;

public partial class Article
{
    [Key]
    public int Id { get; set; }

    public int AuthorId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("Articles")]
    public virtual User Author { get; set; } = null!;

    [InverseProperty("Article")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [ForeignKey("ArticleId")]
    [InverseProperty("ArticlesNavigation")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
