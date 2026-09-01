namespace CarGarage.ViewModels.Marketplace
{
    public class PartForSaleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Phone { get; set; }
        public string? GarageName { get; set; }
        public string? City { get; set; }
        // Who created the listing
        public string? OwnerId { get; set; }
        // e.g. Available, Pending, Sold
        public string? Status { get; set; }
        public DateTime DateAdded { get; set; }

        public string Description { get; set; } = null!;
    }
}
