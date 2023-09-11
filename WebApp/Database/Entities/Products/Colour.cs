using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations.Products;

namespace WebApp.Database.Entities.Products
{
	[EntityTypeConfiguration(typeof(ColourConfiguration))]
	public class Colour
	{
		public int Id { get; set; }

		public string Name { get; set; } = default!;
		public string HexCode { get; set; } = default!;

		public List<ProductStock> Stocks { get; set; } = default!;
	}
}
