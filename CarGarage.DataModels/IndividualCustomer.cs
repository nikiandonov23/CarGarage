using System.ComponentModel.DataAnnotations;

public class IndividualCustomer : Customer
{
    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    [StringLength(10)]
    public string Egn { get; set; } = null!;
}