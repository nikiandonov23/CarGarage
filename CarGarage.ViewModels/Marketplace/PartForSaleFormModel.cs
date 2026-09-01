using CarGarage.ViewModels.Parts;
using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Marketplace
{
    public class PartForSaleFormModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        // For dropdown
        public IEnumerable<PartCategoryViewModel>? Categories { get; set; }
    }
}
