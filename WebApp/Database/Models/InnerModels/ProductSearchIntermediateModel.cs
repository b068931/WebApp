using WebApp.Database.Entities.Products;

namespace WebApp.Database.Models.InnerModels
{
	public class ProductSearchIntermediateModel
	{
		public int Id { get; set; }
		public string Name { get; set; } = default!;

		public decimal Price { get; set; }
		public int Discount { get; set; }
		public decimal TruePrice { get; set; }

		public int TrueRating { get; set; }
		public int ViewsCount { get; set; }

		public string? MainImage { get; set; }
		public DateOnly Created { get; set; }
		public List<ProductStock> Stocks { get; set; } = default!;
	}
}
