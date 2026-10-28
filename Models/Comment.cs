using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Type_It_Out_Backend.Models;

public partial class Comment
{
    [Key]
    public int Id { get; set; }

    public int ArticleId { get; set; }

    public int UserId { get; set; }

    [StringLength(2000)]
    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [ForeignKey("ArticleId")]
    [InverseProperty("Comments")]
    public virtual Article Article { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Comments")]
    public virtual User User { get; set; } = null!;
}
