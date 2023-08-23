using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities;

namespace WebApp.Database.Configurations
{
	public class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
	{
		public void Configure(EntityTypeBuilder<ProductStock> builder)
		{
			builder
				.ToTable("ProductStock")
				.HasKey(e => e.Id);

			builder
				.HasOne(e => e.Product)
				.WithMany(e => e.Stocks)
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			builder
				.HasOne(e => e.Colour)
				.WithMany(e => e.Stocks)
				.HasForeignKey(e => e.ColourId)
				.OnDelete(DeleteBehavior.Restrict);

			builder
				.HasOne(e => e.Size)
				.WithMany(e => e.Stocks)
				.HasForeignKey(e => e.SizeId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
