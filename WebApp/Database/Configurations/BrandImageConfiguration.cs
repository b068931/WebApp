using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Grouping;

namespace WebApp.Database.Configurations
{
    public class BrandImageConfiguration : IEntityTypeConfiguration<BrandImage>
	{
		public void Configure(EntityTypeBuilder<BrandImage> builder)
		{
			builder
				.ToTable("BrandImages")
				.HasKey(c => c.Id);
		}
	}
}
