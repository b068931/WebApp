using WebApp.Database.Models;

namespace WebApp.ViewModels.Product
{
	public class ProductStocksChange
	{
		public int ProductId { get; set; }

		public List<ProductStock> Stocks { get; set; } = default!;
		public string? ErrorMessage { get; set; }

		public List<Colour> Colours { get; set; } = default!;
		public List<Size> Sizes { get; set; } = default!;
	}
}
