namespace WebApp.Database.Models
{
    public class ProductShowLightWeightJson
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;

        public decimal Price { get; set; }
        public int Discount { get; set; }
        public decimal TruePrice { get; set; }

        public int TrueRating { get; set; }

        public DateOnly Date { get; set; }
        public int ViewsCount { get; set; }

        public int MainImageId { get; set; }
    }
}
