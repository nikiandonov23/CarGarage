namespace CarGarage.ViewModels.Cars
{
    public class CarViewModel
    {
        public int Id { get; set; }

        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int ModelYear { get; set; }


       
        
      
 

        public string? RegistrationNumber { get; set; }
        public int? Mileage { get; set; }
        public string? ImageUrl { get; set; }



        public string? Notes { get; set; }
        public DateTime AddedDate { get; set; }

    }
}
