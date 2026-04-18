using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.MyCars
{
    public class CreateCarViewModel
    {
        [Display(Name = "VIN номер")]
        [MaxLength(17, ErrorMessage = "VIN номерът не може да е по-дълъг от 17 символа")]
        public string Vin { get; set; } = string.Empty;

        [Display(Name = "Марка")]
        [Required(ErrorMessage = "Марката е задължителна")]
        [MaxLength(50)]
        public string Make { get; set; } = string.Empty;

        [Display(Name = "Модел")]
        [Required(ErrorMessage = "Моделът е задължителен")]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Display(Name = "Година")]
        [Range(1900, 2100, ErrorMessage = "Годината трябва да бъде между 1900 и 2100")]
        public int ModelYear { get; set; }

        [Display(Name = "Регистрационен номер")]
        [Required(ErrorMessage = "Регистрационният номер е задължителен")]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Пробег (км)")]
        public int? Mileage { get; set; }

        [Display(Name = "URL на снимка")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Бележки")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsPopulatedFromApi { get; set; } = false;
    }
}