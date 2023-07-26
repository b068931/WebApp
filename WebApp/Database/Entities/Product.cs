namespace WebApp.Database.Entities
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;

		public decimal Price { get; set; }
		public int Discount { get; set; }
		public int ViewsCount { get; set; }

		public int StarsCount { get; set; }
		public int RatingsCount { get; set; }

		public int? BrandId { get; set; }
		public Brand? Brand { get; set; }

		public string MainImagePath { get; set; } = default!;
		public List<Image> Images { get; set; } = default!;

		public int CategoryId { get; set; }
		public Category Category { get; set; } = default!;
	}
}
