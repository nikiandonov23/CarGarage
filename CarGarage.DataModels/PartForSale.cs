using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    public class PartForSale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual PartCategory Category { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Which garage lists this part (for city/filtering and owner check)
        [Required]
        public int GarageId { get; set; }

        [ForeignKey(nameof(GarageId))]
        public virtual Garage? Garage { get; set; }

        // The Id of the Identity user who created the listing
        [Required]
        public string OwnerId { get; set; } = null!;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        // Listing status: Available, Pending, Sold
        [MaxLength(50)]
        public string Status { get; set; } = "Available";
    }
}
