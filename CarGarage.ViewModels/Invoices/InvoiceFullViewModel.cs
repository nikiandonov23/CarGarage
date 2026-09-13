using CarGarage.DataModels.Enums;

namespace CarGarage.ViewModels.Invoices
{
    public class InvoiceFullViewModel
    {

        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = null!;
        public DateTime IssuedDate { get; set; }
        public bool IsCancelled { get; set; }



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



        // --- ПОЛУЧАТЕЛ (КЛИЕНТ) ДАННИ ---
        public string? ClientName { get; set; }
        public string? ClientIdNumber { get; set; } // ЕГН за физ. лица / ЕИК за фирми
        public string? ClientAddress { get; set; }   // Град + Адрес
        public string? ClientType { get; set; }      // "Физическо лице" или "Юридическо лице"
        public string? ClientVatNumber { get; set; } // ИН по ЗДДС (ако е фирма и е рег. по ЗДДС)
        public string? ClientMOL { get; set; }       // МОЛ (за фирми)
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }


        // --- ДОБАВЕНИ СВОЙСТВА ЗА СЕРВИЗА (GARAGE) ---
        public string GarageName { get; set; } = null!;
        public string GarageBulstat { get; set; } = null!;
        public string? GarageOwnerName { get; set; }
        public string GarageCity { get; set; } = null!;
        public string GarageAddress { get; set; } = null!;
        public string? GaragePhoneNumber { get; set; }
        public bool GarageIsVatRegistered { get; set; }
        public string? GarageIBAN { get; set; }
        public string? GarageBIC { get; set; }
        public string? GarageBankName { get; set; }
    }
}