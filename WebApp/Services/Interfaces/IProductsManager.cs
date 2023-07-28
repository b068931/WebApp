using WebApp.Database.Entities;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Interfaces
{
	public interface IProductsManager
	{
		Product FindProduct(int productId);
		void CreateProduct(ProductCreate vm);
	}
}
