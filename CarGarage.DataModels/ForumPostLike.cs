using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CarGarage.DataModels
{
    public class ForumPostLike
    {
        public int PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual ForumPost? Post { get; set; }

        public string UserId { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser? User { get; set; }
    }
}
