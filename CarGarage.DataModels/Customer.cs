using CarGarage.DataModels;
using System.ComponentModel.DataAnnotations;

public abstract class Customer
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    public string Address { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    // Връзка към колите - един клиент може да има много автомобили
    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}