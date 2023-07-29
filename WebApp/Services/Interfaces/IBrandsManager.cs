using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Services.Interfaces
{
	public interface IBrandsManager
	{
		List<Database.Models.Brand> GetAllBrands();
		List<SelectListItem> GetSelectList();
		List<SelectListItem> GetSelectListWithSelectedId(int brandId);

		void CreateBrand(string newBrandName);
		void RenameBrand(int id, string newName);
		void DeleteBrand(int id);
	}
}
