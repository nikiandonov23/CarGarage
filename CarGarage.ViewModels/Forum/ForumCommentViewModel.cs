using System;

namespace CarGarage.ViewModels.Forum
{
    public class ForumCommentViewModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string Content { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserDisplayName { get; set; } = null!;
        public string? GarageName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
