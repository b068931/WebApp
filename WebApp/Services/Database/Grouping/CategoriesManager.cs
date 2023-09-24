using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Grouping;
using WebApp.Database.Models;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Categories;

namespace WebApp.Services.Database.Grouping
{
	public class CategoriesManager
	{
		private readonly DatabaseContext _database;
		private Task<int> GetProductsCountInCategoryAsync(int categoryId)
		{
			return _database.Categories
				.Include(e => e.Products)
				.Where(e => e.Id == categoryId)
				.Select(e => e.Products.Count())
				.FirstAsync();
		}

		private async Task<Category> FindCategoryAsync(int categoryId)
		{
			return await _database.Categories.FindAsync(categoryId) ??
				throw new UserInteractionException(
					$"Category with id {categoryId} does not exist."
				);
		}
		private async Task<Category> FindChildWithParentCategoryAsync(int categoryId)
		{
			return await _database.Categories
				.Include(e => e.Parent)
				.FirstOrDefaultAsync(e => e.Id == categoryId)
					?? throw new UserInteractionException(
						$"Category with id {categoryId} does not exist."
					);
		}

		private void LoadCategoryChildren(Category category)
		{
			_database.Categories
					.Entry(category)
					.Collection(e => e.Children)
					.Load();
		}
		private static List<CategoryVM> ConvertChildrenToVM(Category category)
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
		public async Task<List<SelectListItem>> GetSelectListAsync()
		{
			List<Category> categories = await _database.Categories
				.AsNoTracking()
				.Where(e => e.IsLast)
				.ToListAsync();

			List<SelectListItem> categoriesSelectList = new();
			foreach (var category in categories)
			{
				categoriesSelectList.Add(new SelectListItem()
				{
					Value = category.Id.ToString(),
					Text = await BackTrackCategoryAsync(category.Id)
				});
			}

			return categoriesSelectList;
		}
		public async Task<List<SelectListItem>> GetSelectListWithSelectedIdAsync(int categoryId)
		{
			List<SelectListItem> categories = await GetSelectListAsync();
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

		public async Task<Category> GetBaseCategoryAsync()
		{
			Category fakeBaseCategoriesCategory = new()
			{
				Id = 0,
				Name = "[Base Categories]",
				IsLast = false,
				Children = await _database.Categories.Where(e => e.ParentId == null).ToListAsync()
			};

			return fakeBaseCategoriesCategory;
		}
		public Task<List<CategoryJsonModel>> GetCategoriesOnParentAsync(int? parentId)
		{
			return _database.Categories
				.AsNoTracking()
				.Where(e => e.ParentId == parentId)
				.Select(e =>
					new CategoryJsonModel()
					{
						Id = e.Id,
						Name = e.Name,
						IsLast = e.IsLast,
						IsPopular = e.IsPopular
					}
				)
				.ToListAsync();
		}
		public async Task<List<CategoryVM>> GetPopularCategoriesAsync()
		{
			List<Category> popularCategories = await _database.Categories
				.AsNoTracking()
				.Include(e => e.Children)
				.Where(e => e.IsPopular && !e.IsLast)
				.ToListAsync();

			List<CategoryVM> result = new();
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

		public Task<bool> CheckIfLastAsync(int categoryId)
		{
			return _database.Categories
				.Where(e => e.Id == categoryId && e.IsLast)
				.AnyAsync();
		}
		public async Task SwitchPopularityAsync(int categoryId)
		{
			Category foundCategory = await FindCategoryAsync(categoryId);
			foundCategory.IsPopular ^= true;

			await _database.SaveChangesAsync();
		}
		public async Task<string> BackTrackCategoryAsync(int categoryId)
		{
			string fullCategoryName = "";
			Category? foundCategory = await FindCategoryAsync(categoryId);

			do
			{
				fullCategoryName = foundCategory.Name + "/" + fullCategoryName;

				await _database.Entry(foundCategory)
					.Reference(e => e.Parent)
					.LoadAsync();

				foundCategory = foundCategory.Parent;
			} while (foundCategory != null);

			return fullCategoryName;
		}

		public async Task CreateCategoryAsync(int? parentId, string newCategoryName)
		{
			if (parentId != null)
			{
				if (await GetProductsCountInCategoryAsync(parentId.Value) != 0)
					throw new UserInteractionException("You can not create child category for category with products.");

				Category parent = await FindCategoryAsync(parentId.Value);
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

			await _database.SaveChangesAsync();
		}
		public async Task DeleteCategoryAsync(int categoryId)
		{
			Category foundCategory = await FindChildWithParentCategoryAsync(categoryId);
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
			await _database.SaveChangesAsync();
		}
		public async Task MoveCategoryAsync(int categoryId, int newParentId)
		{
			Category foundCategory = await FindChildWithParentCategoryAsync(categoryId);
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
				Category newParent = await FindCategoryAsync(newParentId);
				newParent.IsLast = false;

				foundCategory.ParentId = newParentId;
			}

			await _database.SaveChangesAsync();
		}
		public async Task RenameCategoryAsync(int categoryId, string newName)
		{
			Category foundCategory = await FindCategoryAsync(categoryId);
			foundCategory.Name = newName;

			await _database.SaveChangesAsync();
		}
		public async Task SalvageCategoryAsync(int categoryId, int destinationId)
		{
			using var transaction = await _database.Database.BeginTransactionAsync();
			try
			{
				Category foundCategory = await FindCategoryAsync(categoryId);
				RecursiveSalvageCategory(foundCategory, destinationId);

				await transaction.CommitAsync();
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
	}
}
