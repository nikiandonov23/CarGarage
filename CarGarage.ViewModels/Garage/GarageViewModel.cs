using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Garage
{
    public class GarageViewModel
    {
        [Required(ErrorMessage = "Името на сервиза е задължително")]
        [Display(Name = "Име на сервиза")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Булстат е задължителен")]
        public string Bulstat { get; set; } = null!;

        [Required(ErrorMessage = "Градът е задължителен")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Адресът е задължителен")]
        public string Address { get; set; } = null!;

        [Display(Name = "Телефонен номер")]
        [RegularExpression(@"^(?:\+359|0)\s?8[789]\d\s?\d{3}\s?\d{3}$", ErrorMessage = "Невалиден бг номер")]
        public string? PhoneNumber { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }


        [Display(Name = "Име на собственик")]
        public string? OwnerName { get; set; } // Добавяме това поле
    }
}