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

		List<CategoryJson> GetCategoriesOnParent(int? parentId);
		Category GetBaseCategory();
		bool CheckIfLast(int categoryId);

		void SalvageCategory(int categoryId, int destinationId);
		void CreateCategory(int? parentId, string newCategoryName);
		void RenameCategory(int categoryId, string newName);
		void MoveCategory(int categoryId, int newParentId);
		void DeleteCategory(int categoryId);
	}
}
