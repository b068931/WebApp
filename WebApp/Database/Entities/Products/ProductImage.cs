using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations.Products;

namespace WebApp.Database.Entities.Products
{
	[EntityTypeConfiguration(typeof(ProductImageConfiguration))]
	public class ProductImage
	{
		public int Id { get; set; }
		public string StorageRelativeLocation { get; set; } = default!;

		public int ProductId { get; set; }
		public Product Product { get; set; } = default!;
	}
}
