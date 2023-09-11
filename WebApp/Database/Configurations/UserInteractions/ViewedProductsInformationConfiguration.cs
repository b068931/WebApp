using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Configurations.Abstract;
using WebApp.Database.Entities.UserInteractions;

namespace WebApp.Database.Configurations.UserInteractions
{
	public class ViewedProductsInformationConfiguration : UserProductMappingWithDateConfiguration<ViewedProductsInformation>
	{
		public override void Configure(EntityTypeBuilder<ViewedProductsInformation> builder)
		{
			base.Configure(builder);
			builder.ToTable("ViewedProductsInformation");
		}
	}
}
