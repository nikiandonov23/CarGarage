using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.ViewModels.Customers
{
    public class CustomerDetailsViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string CustomerType { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string UniqueNumber { get; set; } = null!; // ЕГН/ЕИК

        // Коли на клиента
        public List<CustomerCarViewModel> Cars { get; set; } = new();

        // История на ремонтите (Фактури + Части)
        public List<CustomerRepairHistoryViewModel> RepairHistory { get; set; } = new();

        // Обща статистика
        public decimal TotalSpent => RepairHistory.Sum(h => h.TotalAmount);
    }
}
