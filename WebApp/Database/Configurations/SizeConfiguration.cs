using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities;

namespace WebApp.Database.Configurations
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
		}
	}
}
