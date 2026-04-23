
namespace CarGarage.ViewModels.Customers;
public class CustomerInfoViewModel
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = null!; //име на човека или фирмата
    public string CustomerType { get; set; } = null!; // фирма или физ.лице
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string City { get; set; } = null!;
    public string UniqueNumber { get; set; } = null!; //     ЕГН / ЕИК
    public int CarsCount { get; set; }
}