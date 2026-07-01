using CarGarage.ViewModels.Cars.Dropdowns;
using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Cars
{
    public class CreateCarViewModel
    {
        public int Id { get; set; }

        [Display(Name = "VIN номер")]
        [MaxLength(17, ErrorMessage = "VIN номерът не може да е по-дълъг от 17 символа")]
        public string Vin { get; set; } = string.Empty;

        [Display(Name = "Марка (Текст)")]
        [MaxLength(50)]
        public string? Make { get; set; }

        [Display(Name = "Модел (Текст)")]
        [MaxLength(50)]
        public string? Model { get; set; }

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

        [Required(ErrorMessage = "Моля, изберете марка")]
        public int MakeId { get; set; }

        public IEnumerable<CreateCarMakeDropDownViewModel> MakeList { get; set; } =
            new HashSet<CreateCarMakeDropDownViewModel>();

        [Required(ErrorMessage = "Моля, изберете модел")]
        public int ModelId { get; set; }

        public IEnumerable<CreateCarModelDropDownViewModel> ModelList { get; set; } =
            new HashSet<CreateCarModelDropDownViewModel>();

        // --- ЛОГИКА ЗА КЛИЕНТ (СЪЩЕСТВУВАЩ ИЛИ НОВ) ---

        [Display(Name = "Тип клиент")]
        public bool IsNewCustomer { get; set; }

        // За съществуващ клиент
        [Display(Name = "Избор на Клиент (Собственик)")]
        public int? CustomerId { get; set; }

        public IEnumerable<CreateCarCustomerDropDownViewModel> CustomerList { get; set; } =
            new List<CreateCarCustomerDropDownViewModel>();

        // За нов клиент - Общи полета
        [Display(Name = "Вид на новия клиент")]
        public string NewCustomerType { get; set; } = "Individual";

        [Display(Name = "Имейл")]
        [EmailAddress(ErrorMessage = "Невалиден имейл формат")]
        public string? NewCustomerEmail { get; set; }

        [Display(Name = "Телефонен номер")]
        public string? NewCustomerPhoneNumber { get; set; }

        [Display(Name = "Адрес")]
        public string? NewCustomerAddress { get; set; }

        [Display(Name = "Град")]
        public string? NewCustomerCity { get; set; }

        // Полета за Физическо Лице
        [Display(Name = "Име")]
        [MaxLength(50)]
        public string? NewFirstName { get; set; }

        [Display(Name = "Фамилия")]
        [MaxLength(50)]
        public string? NewLastName { get; set; }

        [Display(Name = "ЕГН")]
        [StringLength(10, ErrorMessage = "ЕГН трябва да е точно 10 символа")]
        public string? NewCustomerEgn { get; set; }

        // Полета за Юридическо Лице
        [Display(Name = "Име на фирма")]
        [MaxLength(100)]
        public string? NewCompanyName { get; set; }

        [Display(Name = "ЕИК / БУЛСТАТ")]
        public string? NewCustomerVatNumber { get; set; }

        [Display(Name = "Регистриран по ДДС")]
        public bool NewCustomerIsVatRegistered { get; set; }

        [Display(Name = "МОЛ (Материално отговорно лице)")]
        public string? NewCustomerResponsiblePerson { get; set; }
    }
}