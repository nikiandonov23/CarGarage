using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    public class Part
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public virtual PartCategory Category { get; set; } = null!;

        [Required]
        [MaxLength(250)]
        public string Description { get; set; } = null!; 
        [Required]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } 

        public decimal TotalPrice => Quantity * UnitPrice; 

        
       
        [Required]
        public int CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        public virtual Car Car { get; set; } = null!;


        [MaxLength(500)]
        public string? InvoiceUrl { get; set; } // фактури

        [Required]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow; // Добра практика е да имаш дата на ремонта
    }
}
