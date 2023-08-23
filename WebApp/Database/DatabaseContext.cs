using Microsoft.EntityFrameworkCore;
using WebApp.Database.Entities;

namespace WebApp.Database
{
	public class DatabaseContext : DbContext
	{
		public DatabaseContext(DbContextOptions options) 
			: base(options)
		{
			Database.EnsureCreated();
		}

		public DbSet<Category> Categories { get; set; }
		public DbSet<Brand> Brands { get; set; }

		public DbSet<Product> Products { get; set; }
		public DbSet<ProductStock> ProductStocks { get; set; }
		public DbSet<Colour> ProductColours { get; set; }
		public DbSet<Size> ProductSizes { get; set; }

		public DbSet<ProductImage> ProductImages { get; set; }
		public DbSet<BrandImage> BrandImages { get; set; }
	}
}
