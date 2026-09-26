using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    [Table("PartForSaleImages")]
    public class PartForSaleImage
    {
        [Key]
        public int Id { get; set; }

        public int PartForSaleId { get; set; }

        [ForeignKey(nameof(PartForSaleId))]
        public virtual PartForSale PartForSale { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string StorageKey { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;
    }
}
