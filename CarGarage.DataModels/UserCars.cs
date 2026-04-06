
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarGarage.DataModels
{
    public class UserCars
    {
        [Key, Column(Order = 0)]
        public string UserId { get; set; } = null!;

        [Key, Column(Order = 1)]
        public int CarId { get; set; }

        public IdentityUser User { get; set; } = null!;  

        public Car Car { get; set; } = null!;

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        


    }
}