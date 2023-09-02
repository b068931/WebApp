using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Entities.Auth
{
	[EntityTypeConfiguration(typeof(ApplicationUserConfiguration))]
	public class ApplicationUser : IdentityUser<int>
	{
		public List<Product> UserProducts { get; set; } = default!;

		public List<Product> ViewedProducts { get; set; } = default!;
		public List<Product> RatedProducts { get; set; } = default!;
	}
}
