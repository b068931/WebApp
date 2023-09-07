using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;

namespace WebApp.Database.Entities.Products
{
	[EntityTypeConfiguration(typeof(ProductStockConfiguration))]
	public class ProductStock
	{
		public int Id { get; set; }
		public int ProductAmount { get; set; }

		public int ProductId { get; set; }
		public Product Product { get; set; } = default!;

		public int ColourId { get; set; }
		public Colour Colour { get; set; } = default!;

		public int SizeId { get; set; }
		public Size Size { get; set; } = default!;
	}
}
