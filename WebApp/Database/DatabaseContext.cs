using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Database.Entities.Auth;
using WebApp.Database.Entities.Grouping;
using WebApp.Database.Entities.Products;
using WebApp.Database.Entities.UserInteractions;
using WebApp.Extensions;

namespace WebApp.Database
{
	public class DatabaseContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
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

		public DbSet<ViewedProductsInformation> ViewedProducts { get; set; }
		public DbSet<RatedProductsInformation> RatedProducts { get; set; }

		public DbSet<Colour> ProductColours { get; set; }
		public DbSet<Size> ProductSizes { get; set; }

		public DbSet<ProductImage> ProductImages { get; set; }
		public DbSet<BrandImage> BrandImages { get; set; }

		protected override void ConfigureConventions(ModelConfigurationBuilder builder)
		{
			builder.Properties<DateOnly>()
				.HaveConversion<DateOnlyConverter>()
				.HaveColumnType("Date");

			base.ConfigureConventions(builder);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			//Identity defines its own implementation of this method, so we have to call it.
			//Also i believe it is the only way to change the default names of the (identity) tables.
			base.OnModelCreating(builder);

			builder.Entity<ApplicationUser>().ToTable("Users");
			builder.Entity<ApplicationRole>().ToTable("Roles");
			builder.Entity<ApplicationUserRole>().ToTable("UserRoles");
			builder.Entity<ApplicationUserLogin>().ToTable("UserLogins");
			builder.Entity<ApplicationUserClaim>().ToTable("UserClaims");
			builder.Entity<ApplicationRoleClaim>().ToTable("RoleClaims");
			builder.Entity<ApplicationUserToken>().ToTable("UserTokens");
		}
	}
}
