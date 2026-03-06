using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    [Table("Cars")]
    public class Car
    {
        [Key]
        public int Id { get; set; }

        // Основни от API (идентификация)
        [Display(Name = "VIN номер")]
        [Required, MaxLength(17)]
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

      
     
        // Custom полета
        [Display(Name = "Регистрационен номер")]
        [MaxLength(20)]
        public string? RegistrationNumber { get; set; }

        [Display(Name = "Пробег (км)")]
        public int? Mileage { get; set; }

   

        [Display(Name = "Снимка URL")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Бележки")]
        public string? Notes { get; set; }

        [Display(Name = "Дата на добавяне")]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Many-to-many navigation
        public ICollection<UserCars> UserCars { get; set; } = new List<UserCars>();
    }
}