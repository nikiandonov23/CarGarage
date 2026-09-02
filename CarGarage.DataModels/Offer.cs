using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CarGarage.DataModels
{
    public class Offer
    {
        public int Id { get; set; }

        [Required]
        public int PartForSaleId { get; set; }

        [Required]
        public string SenderId { get; set; } = null!; // user who made the offer

        [Required]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public OfferStatus Status { get; set; } = OfferStatus.Pending;

        // Navigation
        public PartForSale? PartForSale { get; set; }
    }

    public enum OfferStatus
    {
        Pending,
        Accepted,
        Rejected,
        NotPaid
    }
}
