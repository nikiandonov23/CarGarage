using System;
using System.Collections.Generic;

namespace CarGarage.ViewModels.Forum
{
    public class ForumPostViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string UserId { get; set; } = null!;
        public string UserDisplayName { get; set; } = null!;
        public string? GarageName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int CommentsCount { get; set; }
        public List<ForumCommentViewModel> Comments { get; set; } = new List<ForumCommentViewModel>();
    }
}
