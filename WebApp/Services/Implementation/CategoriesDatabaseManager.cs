using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Database.Models;
using WebApp.Helpers;
using WebApp.Services.Interfaces;

namespace WebApp.Services.Implementation
{
	public class CategoriesDatabaseManager : ICategoriesManager
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

		public CategoriesDatabaseManager(DatabaseContext database)
		{
			_database = database;
		}
		
		public CategoriesDrawer GetBrush()
		{
			return new CategoriesDrawer(10, "|\t", '|', "\n", _database);
		}
		public List<SelectListItem> GetSelectList()
		{
			return _database.Categories
				.AsNoTracking()
				.Where(e => e.IsLast)
				.Select(e => new SelectListItem() { Value = e.Id.ToString(), Text = e.Name })
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

		public List<CategoryJson> GetCategoriesOnParent(int? parentId)
		{
			return _database.Categories
				.AsNoTracking()
				.Where(e => e.ParentId == parentId)
				.Select(e => new CategoryJson() { Id = e.Id, Name = e.Name, IsLast = e.IsLast })
				.ToList();
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
