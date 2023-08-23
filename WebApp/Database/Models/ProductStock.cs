namespace WebApp.Database.Models
{
	public class ProductStock
	{
		public int Id { get; set; }
		public int ProductAmount { get; set; }

		public Colour Colour { get; set; } = default!;
		public Size Size { get; set; } = default!;
	}
}
