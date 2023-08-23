using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;

namespace WebApp.Database.Entities
{
	[EntityTypeConfiguration(typeof(BrandImageConfiguration))]
	public class BrandImage
	{
		public int Id { get; set; }

		public byte[] Data { get; set; } = default!;
		public string ContentType { get; set; } = default!;

		public Brand Brand { get; set; } = default!;
	}
}
