using WebApp.Database.Models.Products;

namespace WebApp.ViewModels.Account
{
	public class AccountPageVM
	{
		public string Name { get; set; } = default!;
		public string Email { get; set; } = default!;
		public string CreationDateString { get; set; } = default!;

		public List<ProductPreview> RecentlyViewedProducts { get; set; } = default!;
	}
}
