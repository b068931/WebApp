using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Configurations.Products
{
	public class SizeConfiguration : IEntityTypeConfiguration<Size>
	{
		public void Configure(EntityTypeBuilder<Size> builder)
		{
			builder
				.ToTable("ProductSizes")
				.HasKey(e => e.Id);

			builder
				.HasIndex(e => e.SizeName)
				.IsUnique(true);

			builder
				.Property(e => e.SizeName)
				.HasMaxLength(10);
		}
	}
}
