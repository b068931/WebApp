using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;

namespace WebApp.Database.Entities
{
	[EntityTypeConfiguration(typeof(SizeConfiguration))]
	public class Size
	{
		public int Id { get; set; }
		public string SizeName { get; set; } = default!;

		public List<ProductStock> Stocks { get; set; } = default!;
	}
}
