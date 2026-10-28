using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Type_It_Out_Backend.Models;

[PrimaryKey("FollowerId", "FollowingId")]
public partial class Follow
{
    [Key]
    public int FollowerId { get; set; }

    [Key]
    public int FollowingId { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("FollowerId")]
    [InverseProperty("FollowFollowers")]
    public virtual User Follower { get; set; } = null!;

    [ForeignKey("FollowingId")]
    [InverseProperty("FollowFollowings")]
    public virtual User Following { get; set; } = null!;
}
