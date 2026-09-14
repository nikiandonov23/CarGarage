namespace CarGarage.ViewModels.Garage
{
    public class GarageMapViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string OwnerId { get; set; } = null!;
        public string? OwnerName { get; set; }
    }
}
