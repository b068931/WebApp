namespace WebApp.Database.Models
{
	public class ProductStockModel
	{
		public int Id { get; set; }
		public int ProductAmount { get; set; }

		public ColourModel Colour { get; set; } = default!;
		public SizeModel Size { get; set; } = default!;
	}
}
