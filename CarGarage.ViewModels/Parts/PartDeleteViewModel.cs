namespace CarGarage.ViewModels.Parts
{
    public class PartDeleteViewModel
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string Description { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public decimal TotalPrice { get; set; }
    }
}