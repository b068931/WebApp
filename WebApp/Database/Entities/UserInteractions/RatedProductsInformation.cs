using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations.UserInteractions;
using WebApp.Database.Entities.Abstract;

namespace WebApp.Database.Entities.UserInteractions
{
	[EntityTypeConfiguration(typeof(RatedProductsInformationConfiguration))]
	public class RatedProductsInformation : UserProductMappingWithDate
	{
	}
}
