using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> builder)
		{
			builder
				.ToTable("Products")
				.HasKey(c => c.Id);

			builder
				.HasOne(e => e.Brand)
				.WithMany(e => e.Products)
				.HasForeignKey(e => e.BrandId)
				.OnDelete(DeleteBehavior.SetNull);

			builder
				.HasOne(e => e.Category)
				.WithMany(e => e.Products)
				.HasForeignKey(e => e.CategoryId)
				.OnDelete(DeleteBehavior.Cascade);

			builder
				.HasOne(e => e.MainImage)
				.WithMany()
				.HasForeignKey(e => e.MainImageId)
				.OnDelete(DeleteBehavior.ClientSetNull);

			builder
				.HasOne(e => e.ProductOwner)
				.WithMany(e => e.UserProducts)
				.HasForeignKey(e => e.ProductOwnerId)
				.OnDelete(DeleteBehavior.Cascade);

			builder
				.Property(e => e.Created)
				.HasDefaultValueSql("getdate()");

			builder
				.Property(e => e.Price)
				.HasPrecision(10, 2);

			builder
				.Property(e => e.TruePrice)
				.HasPrecision(10, 2);

			builder
				.Property(e => e.TruePrice)
				.HasComputedColumnSql("[Price] - ([Discount] * [Price] / 100)", stored: true);

			builder
				.Property(e => e.TrueRating)
				.HasComputedColumnSql("COALESCE([StarsCount] / NULLIF([RatingsCount], 0), 0)", stored: true);
		}
	}
}
