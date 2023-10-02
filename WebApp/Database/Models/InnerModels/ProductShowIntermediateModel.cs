namespace WebApp.Database.Models.InnerModels
{
	public class ProductShowIntermediateModel
	{
		public int Id { get; set; }

		public int ProductOwnerId { get; set; }
		public string AuthorName { get; set; } = default!;

		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;

		public decimal Price { get; set; }
		public int Discount { get; set; }
		public decimal TruePrice { get; set; }

		public int ViewsCount { get; set; }
		public int Rating { get; set; }
		public int ReviewsCount { get; set; }

		public string? BrandName { get; set; }
		public int? BrandImageId { get; set; }
		public string? MainImage { get; set; }
	}
}
