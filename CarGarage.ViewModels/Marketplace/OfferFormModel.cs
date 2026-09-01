using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Marketplace
{
    public class OfferFormModel
    {
        public int PartId { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
