using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    [Table("Cars")]
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "VIN номер")]
        [MaxLength(17)]
        public string Vin { get; set; } = string.Empty;

        [Display(Name = "Марка")]
        [Required, MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Display(Name = "Модел")]
        [Required, MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Година")]
        [Range(1900, 2100)]
        public int ModelYear { get; set; }

        [Display(Name = "Регистрационен номер")]
        [Required(ErrorMessage = "Регистрационният номер е задължителен")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Регистрационният номер не може да бъде празен")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Пробег (км)")]
        public int? Mileage { get; set; }

        [Display(Name = "URL на снимка")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Бележки")]
        public string? Notes { get; set; }

        [Display(Name = "Дата на добавяне")]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Many-to-many navigation
        public ICollection<UserCars> UserCars { get; set; } = new List<UserCars>();



        //релаципята ми към частите.1 кола много части
        public virtual ICollection<Part> CarParts { get; set; } = new List<Part>();

        //връзка към фактурите. 1 кола много фактуррри
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();





        // Връзка към клиента - много коли към един клиент
        
        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public virtual Customer? Customer { get; set; } = null!;
    }
}