using WebApp.Database.Models;
using WebApp.ViewModels.Categories;

namespace WebApp.ViewModels.Other
{
	public class AboutUsVM
	{
		public List<CategoryVM> PopularCategories { get; set; } = default!;
		public List<BrandModel> Brands { get; set; } = default!;
	}
}
