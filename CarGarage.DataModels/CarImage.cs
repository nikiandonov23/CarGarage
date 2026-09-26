using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    [Table("CarImages")]
    public class CarImage
    {
        [Key]
        public int Id { get; set; }

        public int CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        public virtual Car Car { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string StorageKey { get; set; } = string.Empty;
    }
}
