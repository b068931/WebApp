using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;
using WebApp.Database.Entities.Auth;
using WebApp.Database.Entities.Grouping;

namespace WebApp.Database.Entities.Products
{
	[EntityTypeConfiguration(typeof(ProductConfiguration))]
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; } = default!;
		public string Description { get; set; } = default!;
		public DateOnly Created { get; set; }

		public decimal Price { get; set; }
		public int Discount { get; set; }
		public decimal TruePrice { get; set; }

		public int ViewsCount { get; set; }

		public int StarsCount { get; set; }
		public int RatingsCount { get; set; }
		public int TrueRating { get; set; }

		public int? BrandId { get; set; }
		public Brand? Brand { get; set; }

		public int? MainImageId { get; set; }
		public ProductImage? MainImage { get; set; }
		public List<ProductImage> Images { get; set; } = default!;

		public int CategoryId { get; set; }
		public Category Category { get; set; } = default!;

		public int ProductOwnerId { get; set; }
		public ApplicationUser ProductOwner { get; set; } = default!;

		public List<ProductStock> Stocks { get; set; } = default!;

		public List<ApplicationUser> ViewedByUsers { get; set; } = default!;
		public List<ApplicationUser> RatedByUsers { get; set; } = default!;
	}
}
