using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Configurations.Products
{
	public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
	{
		public void Configure(EntityTypeBuilder<ProductImage> builder)
		{
			builder
				.ToTable("ProductImages")
				.HasKey(c => c.Id);

			builder
				.HasOne(e => e.Product)
				.WithMany(e => e.Images)
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
