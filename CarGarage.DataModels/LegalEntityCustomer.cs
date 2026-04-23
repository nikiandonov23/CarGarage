using System.ComponentModel.DataAnnotations;

public class LegalEntityCustomer : Customer
{
    [Required]
    public string CompanyName { get; set; } = null!;

    [Required]
    public string VatNumber { get; set; } = null!; // ЕИК / БУЛСТАТ

    public bool IsVatRegistered { get; set; }

    [Required]
    public string ResponsiblePerson { get; set; } = null!; // МОЛ
}