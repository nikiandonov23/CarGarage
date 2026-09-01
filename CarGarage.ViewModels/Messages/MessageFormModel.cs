using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Messages
{
    public class MessageFormModel
    {
        public int? PartId { get; set; }
        public string? ConversationId { get; set; }

        [Required]
        public string ReceiverId { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = null!;
    }
}
