using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Customers
{
    public class CustomerFormViewModel
    {
        public int? Id { get; set; } // null при създаване, със стойност при Edit

        [Required(ErrorMessage = "Типът на клиента е задължителен")]
        public string CustomerType { get; set; } = "Individual"; // "Individual" или "Legal"

        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден имейл")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Телефонът е задължителен")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Адресът е задължителен")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Градът е задължителен")]
        public string City { get; set; } = null!;

        public int GarageId { get; set; }

        // Полета за Физическо лице
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Egn { get; set; }

        // Полета за Юридическо лице
        public string? CompanyName { get; set; }
        public string? VatNumber { get; set; }
        public bool IsVatRegistered { get; set; }
        public string? ResponsiblePerson { get; set; }
    }
}