using CarGarage.ViewModels.Parts;
using System.Collections.Generic;

namespace CarGarage.ViewModels.Marketplace
{
    public class MarketplaceIndexViewModel
    {
        public IEnumerable<PartForSaleViewModel> Results { get; set; } = new List<PartForSaleViewModel>();
        public string? SearchTerm { get; set; }
        public string? City { get; set; }
        public int? CategoryId { get; set; }
        public string? OwnerId { get; set; }
        public IEnumerable<PartCategoryViewModel>? Categories { get; set; }
    }
}
