using WebApp.Database.Entities;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Interfaces
{
	public interface IProductsManager
	{
		Product FindProduct(int productId);
		int GetProductMainImage(int productId);

		ProductShow GetProductShowVM(int productId);
		ProductUpdate GetProductUpdateVM(int productId);
		ProductCreate GetProductCreateVM();

		Product CreateProduct(ProductCreate vm);
		void UpdateProduct(ProductUpdate vm);
		void DeleteProduct(int productId);
		void ChangeMainImage(int productId, int newMainImageId);
	}
}
