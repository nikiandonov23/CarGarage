using System;
using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.MyCars
{
    public class CreateCarViewModel
    {
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




        [Display(Name = "Бележки")]
        [MaxLength(500)]
        public string? Notes { get; set; }



        [Display(Name = "Регистрационен номер")]
        [MaxLength(20)]
        public string? RegistrationNumber { get; set; }

        [Display(Name = "Пробег (км)")]
        public int? Mileage { get; set; }

        [Display(Name = "URL на снимка")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsPopulatedFromApi { get; set; } = false;
    }
}
