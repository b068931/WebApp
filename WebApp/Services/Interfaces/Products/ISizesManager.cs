using WebApp.Database.Models;

namespace WebApp.Services.Interfaces.Products
{
	public interface ISizesManager
	{
		List<Size> GetAllSizes();

		void CreateSize(string sizeName);
		void UpdateSize(int id, string sizeName);
		void DeleteSize(int id);
	}
}
