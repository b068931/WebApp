using WebApp.Database.Models.Products;

namespace WebApp.ViewModels.Account
{
	public class MyProductsVM
	{
		public List<ProductPreview> ProductsList { get; set; } = default!;
	}
}
