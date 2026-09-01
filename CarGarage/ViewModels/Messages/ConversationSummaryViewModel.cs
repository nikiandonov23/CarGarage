using System;

namespace CarGarage.ViewModels.Messages
{
    public class ConversationSummaryViewModel
    {
        public int LatestMessageId { get; set; }
        public string? ConversationId { get; set; }
        public string OtherUserId { get; set; } = null!;
        public string? OtherUserName { get; set; }
        public string? LastMessage { get; set; }
        public DateTime LastSentAt { get; set; }
        public int UnreadCount { get; set; }
        public bool IsPinned { get; set; }
    }
}
