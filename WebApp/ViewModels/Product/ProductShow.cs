using WebApp.Database.Models.Images;
using WebApp.Database.Models.Stocks;

namespace WebApp.ViewModels.Product
{
	public class ProductShow
	{
		public const int MaxStarsRating = 10;

		public int Id { get; set; }
		public bool DisplayEditing { get; set; }
		public string AuthorName { get; set; } = default!;

		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;

		public decimal Price { get; set; }
		public int Discount { get; set; }
		public decimal TruePrice { get; set; }

		public int ViewsCount { get; set; }

		public int Rating { get; set; }
		public int ReviewsCount { get; set; }

		public (string Name, int ImageId)? BrandInfo { get; set; }

		public string MainImage { get; set; } = default!;
		public List<ProductImageModel> ProductImages { get; set; } = default!;

		public List<ColourModel> AvailableColours { get; set; } = default!;
		public List<SizeModel> AvailableSizes { get; set; } = default!;
	}
}
