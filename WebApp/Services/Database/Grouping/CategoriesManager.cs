using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Grouping;
using WebApp.Database.Models;
using WebApp.Utilities;
using WebApp.ViewModels.Categories;

namespace WebApp.Services.Database.Grouping
{
	public class CategoriesManager
	{
		private readonly DatabaseContext _database;

		private Category FindCategory(int categoryId)
		{
			return _database.Categories.Find(categoryId) ??
				throw new ArgumentOutOfRangeException(string.Format("Category with id {0} does not exist.", categoryId));
		}
		private Category FindChildWithParentCategory(int categoryId)
		{
			return _database.Categories
				.Include(e => e.Parent)
				.FirstOrDefault(e => e.Id == categoryId) ?? throw new ArgumentOutOfRangeException(string.Format("Category with id {0} does not exist.", categoryId));
		}

		private void LoadCategoryChildren(Category category)
		{
			_database.Categories
					.Entry(category)
					.Collection(e => e.Children)
					.Load();
		}
		private List<CategoryVM> ConvertChildrenToVM(Category category)
		{
			if (!category.Children.TrueForAll(e => e.IsLast))
				throw new ArgumentException("This function is non recursive. All children must have IsLast == true");

			return category.Children
				.Select(e => new CategoryVM()
				{
					Id = e.Id,
					IsLast = e.IsLast,
					IsPopular = e.IsPopular,
					Name = e.Name,
					SubCategories = new List<CategoryVM>()
				})
				.ToList();
		}

		private void RecursiveRemoveCategory(Category category)
		{
			if (!category.IsLast)
			{
				LoadCategoryChildren(category);
				foreach (Category child in category.Children)
				{
					RecursiveRemoveCategory(child);
				}
			}

			_database.Categories.Remove(category);
		}
		private void RecursiveSalvageCategory(Category category, int destinationId)
		{
			if (!category.IsLast) //We are not particularly interested in parent categories. That is, in categories that can not contain any products
			{
				LoadCategoryChildren(category);
				foreach (Category child in category.Children)
				{
					RecursiveSalvageCategory(child, destinationId);
				}
			}
			else
			{
				_database.Products
					.Where(e => e.CategoryId == category.Id)
					.ExecuteUpdate(
						setters => setters.SetProperty(e => e.CategoryId, destinationId)
					);
			}
		}

		public CategoriesManager(DatabaseContext database)
		{
			_database = database;
		}

		public CategoriesDrawer GetBrush()
		{
			return new CategoriesDrawer(10, "|\t", '|', "\n", _database);
		}
		public List<SelectListItem> GetSelectList()
		{
			List<Category> categories = _database.Categories
				.AsNoTracking()
				.Where(e => e.IsLast)
				.ToList();

			return categories
				.Select(e => new SelectListItem() { Value = e.Id.ToString(), Text = BackTrackCategory(e.Id) })
				.ToList();
		}
		public List<SelectListItem> GetSelectListWithSelectedId(int categoryId)
		{
			List<SelectListItem> categories = GetSelectList();
			foreach (var category in categories)
			{
				if (category.Value == categoryId.ToString())
				{
					category.Selected = true;
					break;
				}
			}

			return categories;
		}

		public Category GetBaseCategory()
		{
			Category fakeBaseCategoriesCategory = new Category()
			{
				Id = 0,
				Name = "[Base Categories]",
				IsLast = false,
				Children = _database.Categories.Where(e => e.ParentId == null).ToList()
			};

			return fakeBaseCategoriesCategory;
		}
		public List<CategoryJson> GetCategoriesOnParent(int? parentId)
		{
			return _database.Categories
				.AsNoTracking()
				.Where(e => e.ParentId == parentId)
				.Select(e =>
					new CategoryJson()
					{
						Id = e.Id,
						Name = e.Name,
						IsLast = e.IsLast,
						IsPopular = e.IsPopular
					}
				)
				.ToList();
		}
		public List<CategoryVM> GetPopularCategories()
		{
			List<Category> popularCategories = _database.Categories
				.AsNoTracking()
				.Include(e => e.Children)
				.Where(e => e.IsPopular && !e.IsLast)
				.ToList();

			List<CategoryVM> result = new List<CategoryVM>();
			foreach (var category in popularCategories)
			{
				result.Add(new CategoryVM()
				{
					Id = category.Id,
					Name = category.Name,
					IsLast = category.IsLast,
					IsPopular = category.IsPopular,
					SubCategories = ConvertChildrenToVM(category)
				});
			}

			return result;
		}

		public bool CheckIfLast(int categoryId)
		{
			return _database.Categories
				.Where(e => e.Id == categoryId && e.IsLast)
				.Count() > 0;
		}
		public void SwitchPopularity(int categoryId)
		{
			Category foundCategory = FindCategory(categoryId);
			foundCategory.IsPopular ^= true;

			_database.SaveChanges();
		}
		public string BackTrackCategory(int categoryId)
		{
			string fullCategoryName = "";
			Category? foundCategory = FindCategory(categoryId);

			do
			{
				fullCategoryName = foundCategory.Name + "/" + fullCategoryName;

				_database.Entry(foundCategory)
					.Reference(e => e.Parent)
					.Load();

				foundCategory = foundCategory.Parent;
			} while (foundCategory != null);

			return fullCategoryName;
		}

		public void CreateCategory(int? parentId, string newCategoryName)
		{
			if (parentId != null)
			{
				Category parent = FindCategory(parentId.Value);
				parent.IsLast = false;
			}

			_database.Categories.Add(
				new Category()
				{
					Name = newCategoryName,
					ParentId = parentId,
					IsLast = true
				}
			);

			_database.SaveChanges();
		}
		public void DeleteCategory(int categoryId)
		{
			Category foundCategory = FindChildWithParentCategory(categoryId);
			Category? parent = foundCategory.Parent;
			if (parent != null)
			{
				LoadCategoryChildren(parent);
				if (parent.Children.Count == 1)
				{
					parent.IsLast = true;
				}
			}

			RecursiveRemoveCategory(foundCategory);
			_database.SaveChanges();
		}
		public void MoveCategory(int categoryId, int newParentId)
		{
			Category foundCategory = FindChildWithParentCategory(categoryId);
			if (foundCategory.Parent != null)
			{
				Category oldParent = foundCategory.Parent;
				LoadCategoryChildren(oldParent);

				if (oldParent.Children.Count == 1)
				{
					oldParent.IsLast = true;
				}
			}

			if (newParentId == 0)
			{
				foundCategory.ParentId = null;
			}
			else
			{
				Category newParent = FindCategory(newParentId);
				newParent.IsLast = false;

				foundCategory.ParentId = newParentId;
			}

			_database.SaveChanges();
		}
		public void RenameCategory(int categoryId, string newName)
		{
			Category foundCategory = FindCategory(categoryId);
			foundCategory.Name = newName;

			_database.SaveChanges();
		}
		public void SalvageCategory(int categoryId, int destinationId)
		{
			using (var transaction = _database.Database.BeginTransaction())
			{
				try
				{
					Category foundCategory = FindCategory(categoryId);
					RecursiveSalvageCategory(foundCategory, destinationId);

					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
					throw;
				}
			}
		}
	}
}
