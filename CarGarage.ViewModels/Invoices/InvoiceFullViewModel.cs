using CarGarage.DataModels.Enums;

namespace CarGarage.ViewModels.Invoices
{
    public class InvoiceFullViewModel
    {

        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = null!;
        public DateTime IssuedDate { get; set; }



        // --- НОВИ СВОЙСТВА ЗА ПЛАЩАНЕТО ---
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodText { get; set; } = null!; // "В брой", "С карта", "По банков път"



        public string CarInfo { get; set; } = null!;
        public string Vin { get; set; } = null!;
        public string RegNumber { get; set; } = null!;
        public List<InvoicePartViewModel> Parts { get; set; } = new();
        public decimal LaborTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Notes { get; set; }


        public int CarId { get; set; }



        // --- ДОБАВЕНИ СВОЙСТВА ЗА СЕРВИЗА (GARAGE) ---
        public string GarageName { get; set; } = null!;
        public string GarageBulstat { get; set; } = null!;
        public string? GarageOwnerName { get; set; }
        public string GarageCity { get; set; } = null!;
        public string GarageAddress { get; set; } = null!;
        public string? GaragePhoneNumber { get; set; }
    }
}