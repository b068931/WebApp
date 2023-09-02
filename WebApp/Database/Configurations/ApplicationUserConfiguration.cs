using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Auth;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Configurations
{
	public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
	{
		public void Configure(EntityTypeBuilder<ApplicationUser> builder)
		{
			builder
				.HasMany(e => e.ViewedProducts)
				.WithMany(e => e.ViewedByUsers)
				.UsingEntity<Dictionary<string, object>>(
					"ViewedProductsInformation",
					r => r.HasOne<Product>()
						.WithMany()
						.HasForeignKey("ProductId")
						.OnDelete(DeleteBehavior.Cascade),
					l => l.HasOne<ApplicationUser>()
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.NoAction),
					e =>
					{
						e.Property<DateOnly>("UsedDate")
							.HasColumnName("UsedDate")
							.HasDefaultValueSql("getdate()");

						e.HasKey("ProductId", "UserId");
					}
				);

			builder
				.HasMany(e => e.RatedProducts)
				.WithMany(e => e.RatedByUsers)
				.UsingEntity<Dictionary<string, object>>(
					"RatedProductsInformation",
					r => r.HasOne<Product>()
						.WithMany()
						.HasForeignKey("ProductId")
						.OnDelete(DeleteBehavior.Cascade),
					l => l.HasOne<ApplicationUser>()
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.NoAction),
					e =>
					{
						e.Property<DateOnly>("UsedDate")
							.HasColumnName("UsedDate")
							.HasDefaultValueSql("getdate()");

						e.HasKey("ProductId", "UserId");
					}
				);
		}
	}
}
