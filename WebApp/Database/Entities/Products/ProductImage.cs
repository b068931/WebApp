using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;

namespace WebApp.Database.Entities.Products
{
	[EntityTypeConfiguration(typeof(ProductImageConfiguration))]
	public class ProductImage
	{
		public int Id { get; set; }
		public byte[] Data { get; set; } = default!;
		public string ContentType { get; set; } = default!;

		public int ProductId { get; set; }
		public Product Product { get; set; } = default!;
	}
}
