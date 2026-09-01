using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.DataModels
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = null!;

        [Required]
        public string ReceiverId { get; set; } = null!;

        [MaxLength(2000)]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public bool IsPinned { get; set; } = false;

        // Threading can be implemented via ConversationId
        public string? ConversationId { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; } = false;
    }
}
