using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Auth;
using WebApp.Database.Entities.UserInteractions;

namespace WebApp.Database.Configurations.Auth
{
	public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
	{
		public void Configure(EntityTypeBuilder<ApplicationUser> builder)
		{
			builder
				.Property(e => e.AccountCreationDate)
				.HasDefaultValueSql("getdate()");

			builder
				.HasMany(e => e.ViewedProducts)
				.WithMany(e => e.ViewedByUsers)
				.UsingEntity<ViewedProductsInformation>(
					r => r.HasOne(e => e.Product)
						.WithMany()
						.HasForeignKey("ProductId"),
					l => l.HasOne(e => e.AssociatedUser)
						.WithMany()
						.HasForeignKey("UserId")
				);

			builder
				.HasMany(e => e.RatedProducts)
				.WithMany(e => e.RatedByUsers)
				.UsingEntity<RatedProductsInformation>(
					r => r.HasOne(e => e.Product)
						.WithMany()
						.HasForeignKey("ProductId"),
					l => l.HasOne(e => e.AssociatedUser)
						.WithMany()
						.HasForeignKey("UserId")
				);
		}
	}
}
