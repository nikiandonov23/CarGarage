namespace CarGarage.ViewModels.MyCars
{
    public class CarViewModel
    {
        public int Id { get; set; }

        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int ModelYear { get; set; }

        public string? Trim { get; set; }
        public string? FuelTypePrimary { get; set; }
        public int? EngineHP { get; set; }
        public string? DriveType { get; set; }

        public string? RegistrationNumber { get; set; }
        public int? Mileage { get; set; }
        public string? ImageUrl { get; set; }

    }
}
