using CarGarage.DataModels.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = null!; // номкер ф ра

        [Required]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;


        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash; // <-- НОВО ПРОПЪРТИ


        // Връзка към колата
        [Required]
        public int CarId { get; set; }
        [ForeignKey(nameof(CarId))]
        public virtual Car Car { get; set; } = null!;

        // Труд
        [Column(TypeName = "decimal(18,2)")]
        public decimal LaborPricePerHour { get; set; }

        public double LaborHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalLaborPrice => LaborPricePerHour * (decimal)LaborHours;

        [Column(TypeName = "decimal(18,2)")] 
        public decimal TaxPercentage { get; set; } = 20; // ДДС 

       
        public virtual ICollection<Part> Parts { get; set; } = new HashSet<Part>();

        
        public string? Notes { get; set; }



        // В Invoice.cs
        [Required]
        public int GarageId { get; set; }

        [ForeignKey(nameof(GarageId))]
        public virtual Garage? Garage { get; set; }
    }
}