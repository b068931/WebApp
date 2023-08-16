using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Database.Entities;
using WebApp.Database.Models;
using WebApp.Helpers;

namespace WebApp.Services.Interfaces
{
	public interface ICategoriesManager
	{
		CategoriesDrawer GetBrush();
		List<SelectListItem> GetSelectList();
		List<SelectListItem> GetSelectListWithSelectedId(int categoryId);

		Category GetBaseCategory();
		List<CategoryJson> GetCategoriesOnParent(int? parentId);
		List<CategoryJson> GetRandomCategories(int count);

		bool CheckIfLast(int categoryId);
		void SwitchPopularity(int categoryId);

		void SalvageCategory(int categoryId, int destinationId);
		void CreateCategory(int? parentId, string newCategoryName);
		void RenameCategory(int categoryId, string newName);
		void MoveCategory(int categoryId, int newParentId);
		void DeleteCategory(int categoryId);
	}
}
