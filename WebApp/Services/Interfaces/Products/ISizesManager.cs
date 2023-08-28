using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Database.Models;

namespace WebApp.Services.Interfaces.Products
{
	public interface ISizesManager
	{
		List<Size> GetAllSizes();
		List<SelectListItem> GetSelectList();

		void CreateSize(string sizeName);
		void UpdateSize(int id, string sizeName);
		void DeleteSize(int id);
	}
}
