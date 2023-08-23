using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using WebApp.Database.Entities;

namespace WebApp.Database.Configurations
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
