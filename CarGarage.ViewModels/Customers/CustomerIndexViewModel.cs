
using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Customers;

public class CustomerIndexViewModel
{
    public string? SearchTerm { get; set; }
    public IEnumerable<CustomerInfoViewModel> Customers { get; set; } = new List<CustomerInfoViewModel>();
}