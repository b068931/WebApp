using Microsoft.AspNetCore.Mvc;
using WebApp.Database;
using WebApp.Database.Models;
using System.Text.Json;
using WebApp.Database.Entities;
using WebApp.Helpers;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
	[Route("/categories")]
	public class CategoriesController : Controller
	{
		private readonly DatabaseContext _database;
		private readonly ILogger<CategoriesController> _logger;

		private Category GetFakeBaseCategory()
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
		private CategoriesDrawer GetBrush()
		{
			return new CategoriesDrawer(10, "|\t", '|', "\n", _database);
		}
		private (string, string) GenerateAdminPageModel(string result)
		{
			return (GetBrush().DrawCategories(GetFakeBaseCategory()), result);
		}

		private List<CategoryJson> RequestCategoriesOnParent(int? parentId)
		{
			return _database.Categories
				.Where(e => e.ParentId == parentId)
				.Select(e => new CategoryJson() { Id = e.Id, Name = e.Name, IsLast = e.IsLast })
				.ToList();
		}
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

		private IActionResult PerformAction(Action callback, string operationName)
		{
			string result = "'" + operationName + "' operation success.";
			try
			{
				callback();
			}
			catch (ArgumentOutOfRangeException ex)
			{
				result = ex.Message;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected exception ({0}).", operationName);
				result = "Unknown exception.";
			}

			return View("AdminPage", GenerateAdminPageModel(result));
		}
		private void LoadCategoryChildren(Category category)
		{
			_database.Categories
					.Entry(category)
					.Collection(e => e.Children)
					.Load();
		}

		private void RecursiveSalvageCategory(Category category, int destinationId)
		{
			if(!category.IsLast) //We are not particularly interested in parent categories that is, in categories that can not contain any products
			{
				LoadCategoryChildren(category);
				foreach(Category child in category.Children)
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
		private void SalvageCategory(int categoryId, int destinationId)
		{
			using (var transaction = _database.Database.BeginTransaction())
			{
				try
				{
					Category foundCategory = FindCategory(categoryId);
					RecursiveSalvageCategory(foundCategory, destinationId);

					transaction.Commit();
				}
				catch(Exception)
				{
					transaction.Rollback();
					throw;
				}
			}
		}

		private void CreateCategory(int? parentId, string newCategoryName)
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
		private void ChangeCategoryName(int categoryId, string newName)
		{
			Category foundCategory = FindCategory(categoryId);
			foundCategory.Name = newName;

			_database.SaveChanges();
		}
		private void MoveCategory(int categoryId, int newParentId)
		{
			Category foundCategory = FindChildWithParentCategory(categoryId);
			if(foundCategory.Parent != null)
			{
				Category oldParent = foundCategory.Parent;
				LoadCategoryChildren(oldParent);

				if(oldParent.Children.Count == 1)
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
		private void DeleteCategory(int categoryId)
		{
			Category foundCategory = FindChildWithParentCategory(categoryId);
			Category? parent = foundCategory.Parent;
			if (parent != null)
			{
				LoadCategoryChildren(parent);
				if(parent.Children.Count == 1)
				{
					parent.IsLast = true;
				}
			}

			RecursiveRemoveCategory(foundCategory);
			_database.SaveChanges();
		}

		public CategoriesController(ILogger<CategoriesController> logger, DatabaseContext database)
		{
			_logger = logger;
			_database = database;
		}

		[HttpGet("base")]
		public JsonResult GetBaseCategories()
		{
			return Json(
				RequestCategoriesOnParent(null),
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
			);
		}

		[HttpGet("category/{parentId}/children")]
		public JsonResult GetChildren(
			[FromRoute(Name = "parentId")] int parentId)
		{
			return Json(
				RequestCategoriesOnParent(parentId),
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
			);
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			[FromForm(Name = "categoryName")] string newCategoryName,
			[FromForm(Name = "parentId")] int parentId)
		{
			return PerformAction(
				() => CreateCategory((parentId == 0) ? null : parentId, newCategoryName), 
				"create"
			);
		}

		[HttpPost("action/rename")]
		[ValidateAntiForgeryToken]
		public IActionResult Rename(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "categoryName")] string newName)
		{
			return PerformAction(() => ChangeCategoryName(categoryId, newName), "rename");
		}

		[HttpPost("action/move")]
		[ValidateAntiForgeryToken]
		public IActionResult Rename(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "parentId")] int parentId)
		{
			return PerformAction(() => MoveCategory(categoryId, parentId), "move");
		}

		[HttpPost("action/salvage")]
		[ValidateAntiForgeryToken]
		public IActionResult Salvage(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "parentId")] int destinationId)
		{
			return PerformAction(() => SalvageCategory(categoryId, destinationId), "salvage");
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "categoryId")] int categoryId)
		{
			return PerformAction(() => DeleteCategory(categoryId), "delete");
		}

		public IActionResult Index()
		{
			return View("AdminPage", GenerateAdminPageModel("..."));
		}
	}
}