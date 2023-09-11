using WebApp.Database.Models;

namespace WebApp.ViewModels.Product
{
	public class ProductStocksChange
	{
		public int ProductId { get; set; }

		public List<ProductStockModel> Stocks { get; set; } = default!;
		public string? ErrorMessage { get; set; }

		public List<ColourModel> Colours { get; set; } = default!;
		public List<SizeModel> Sizes { get; set; } = default!;
	}
}
