using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Configurations.Abstract;
using WebApp.Database.Entities.UserInteractions;

namespace WebApp.Database.Configurations.UserInteractions
{
	public class RatedProductsInformationConfiguration : UserProductMappingWithDateConfiguration<RatedProductsInformation>
	{
		public override void Configure(EntityTypeBuilder<RatedProductsInformation> builder)
		{
			base.Configure(builder);
			builder.ToTable("RatedProductsInformation");
		}
	}
}
