using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Services.Database.Grouping;

namespace WebApp.Controllers.Grouping
{
	[Route("/categories")]
	[Authorize(Roles = "admin")]
	public class CategoriesController : Controller
	{
		private readonly CategoriesManager _categories;
		private readonly ILogger<CategoriesController> _logger;

		private (string, string) GenerateAdminPageModel(string result)
		{
			return (
				_categories.GetBrush().DrawCategories(_categories.GetBaseCategory()),
				result
			);
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
				_logger.LogError(ex, "CategoriesController error. Unexpected exception ({0}).", operationName);
				result = "Unknown exception.";
			}

			return View("AdminPage", GenerateAdminPageModel(result));
		}

		public CategoriesController(CategoriesManager categories, ILogger<CategoriesController> logger)
		{
			_categories = categories;
			_logger = logger;
		}

		[HttpGet("base")]
		[AllowAnonymous]
		public JsonResult GetBaseCategories()
		{
			return Json(
				_categories.GetCategoriesOnParent(null),
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
			);
		}

		[HttpGet("category/{parentId}/children")]
		[AllowAnonymous]
		public JsonResult GetChildren(
			[FromRoute(Name = "parentId")] int parentId)
		{
			return Json(
				_categories.GetCategoriesOnParent(parentId),
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
			);
		}

		[HttpPost("action/switch")]
		[ValidateAntiForgeryToken]
		public IActionResult SwitchPopularity(
			[FromForm(Name = "categoryId")] int categoryId)
		{
			return PerformAction(() => _categories.SwitchPopularity(categoryId), "switch popularity");
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			[FromForm(Name = "categoryName")] string newCategoryName,
			[FromForm(Name = "parentId")] int parentId)
		{
			return PerformAction(
				() => _categories.CreateCategory(parentId == 0 ? null : parentId, newCategoryName),
				"create"
			);
		}

		[HttpPost("action/rename")]
		[ValidateAntiForgeryToken]
		public IActionResult Rename(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "categoryName")] string newName)
		{
			return PerformAction(() => _categories.RenameCategory(categoryId, newName), "rename");
		}

		[HttpPost("action/move")]
		[ValidateAntiForgeryToken]
		public IActionResult Rename(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "parentId")] int parentId)
		{
			return PerformAction(() => _categories.MoveCategory(categoryId, parentId), "move");
		}

		[HttpPost("action/salvage")]
		[ValidateAntiForgeryToken]
		public IActionResult Salvage(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "parentId")] int destinationId)
		{
			return PerformAction(() => _categories.SalvageCategory(categoryId, destinationId), "salvage");
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "categoryId")] int categoryId)
		{
			return PerformAction(() => _categories.DeleteCategory(categoryId), "delete");
		}

		public IActionResult Index()
		{
			return View("AdminPage", GenerateAdminPageModel("..."));
		}
	}
}