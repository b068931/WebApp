using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Database.Entities;

namespace WebApp.Services.Interfaces
{
	public interface IBrandsManager
	{
		List<Database.Models.Brand> GetAllBrands();
		List<SelectListItem> GetSelectList();
		List<SelectListItem> GetSelectListWithSelectedId(int brandId);

		void CreateBrand(string newBrandName, IFormFile brandImage);
		void UpdateBrand(int id, string newName, IFormFile? brandImage);
		void DeleteBrand(int id);
		Task<BrandImage> GetBrandImage(int brandImageId);
	}
}
