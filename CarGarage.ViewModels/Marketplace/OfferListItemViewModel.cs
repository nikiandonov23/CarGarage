using CarGarage.DataModels;

namespace CarGarage.ViewModels.Marketplace
{
    public class OfferListItemViewModel
    {
        public int Id { get; set; }

        public string? PartName { get; set; }

        public string SenderDisplayName { get; set; } = null!;

        public decimal Amount { get; set; }

        public string? Message { get; set; }

        public OfferStatus Status { get; set; }
    }
}
