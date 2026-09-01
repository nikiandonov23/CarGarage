namespace CarGarage.ViewModels.Marketplace
{
    public class PartForSaleDeleteViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public decimal Price { get; set; }
        public int GarageId { get; set; }
    }
}
