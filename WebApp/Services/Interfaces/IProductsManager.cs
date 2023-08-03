using WebApp.Database.Entities;
using WebApp.Database.Models;
using WebApp.Helpers;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Interfaces
{
	public interface IProductsManager
	{
		Product FindProduct(int productId);
		int GetProductMainImage(int productId);

		List<ProductShowLightWeightJson> Search(
			List<IFilter<Product>> filters,
			int currentMaxId
		);

		ProductShow GetProductShowVM(int productId);
		ProductUpdate GetProductUpdateVM(int productId);
		ProductCreate GetProductCreateVM();

		Product CreateProduct(ProductCreate vm);
		void UpdateProduct(ProductUpdate vm);
		void DeleteProduct(int productId);
		void ChangeMainImage(int productId, int newMainImageId);
	}
}
