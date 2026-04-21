using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Description { get; set; } = null!; // Тук потребителят пише: "Предни дискове Brembo"

        [Required]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Потребителят сам въвежда цената

        public decimal TotalPrice => Quantity * UnitPrice; // Изчислено поле

        
        // Връзка към Колата (Foreign Key)
        [Required]
        public int CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        public virtual Car Car { get; set; } = null!;
    }
}
