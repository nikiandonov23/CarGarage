using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CarGarage.DataModels
{
    public class ForumPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = null!;

        public string? ImageUrl { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ForumComment> Comments { get; set; } = new HashSet<ForumComment>();
        public virtual ICollection<ForumPostLike> Likes { get; set; } = new HashSet<ForumPostLike>();
    }
}
