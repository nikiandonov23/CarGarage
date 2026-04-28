using CarGarage.DataModels;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Garage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string? Name { get; set; } // Маркиран като nullable за компилатора

    [Required]
    [StringLength(20)]
    public string? Bulstat { get; set; }

    public string? OwnerName { get; set; }

    [Required]
    public string? City { get; set; }

    [Required]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Longitude { get; set; }



    public string? PhoneNumber { get; set; }
    public string? LogoUrl { get; set; }

    // Връзка към юзъра
    [Required]
    public string? OwnerId { get; set; }

    [ForeignKey("OwnerId")]
    public virtual IdentityUser? Owner { get; set; }

    // Колекциите винаги е добре да се инициализират, за да не гърми кода при итерация
    public virtual ICollection<Invoice> Invoices { get; set; } = new HashSet<Invoice>();
    public virtual ICollection<Part> Parts { get; set; } = new HashSet<Part>();
    public virtual ICollection<Customer> Customers { get; set; } = new HashSet<Customer>();
}