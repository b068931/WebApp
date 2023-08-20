namespace WebApp.Database.Models
{
    public class ProductShowLightWeightJson
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;

        public decimal Price { get; set; }
        public int Discount { get; set; }

        public int Stars { get; set; }
        public DateOnly Date { get; set; }
        public int ViewsCount { get; set; }

        public int MainImageId { get; set; }
    }
}
