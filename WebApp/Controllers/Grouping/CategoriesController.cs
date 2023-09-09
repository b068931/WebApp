using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Grouping
{
	[Route("/categories")]
	[Authorize(Policy = "CriticalSiteContentPolicy")]
	public class CategoriesController : Controller
	{
		private readonly CategoriesManager _categories;
		private readonly Performer<CategoriesController> _performer;

		private async Task<ResultWithErrorVM<string>> GenerateAdminPageModelAsync(string error)
		{
			return new()
			{
				Result = _categories.GetBrush().DrawCategories(
					await _categories.GetBaseCategoryAsync()
				),
				Error = error
			};
		}
		private Task<IActionResult> PerformActionAsync(Func<Task> callback, string operationName)
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					await callback();
					return View(
						"AdminPage",
						await GenerateAdminPageModelAsync(
							string.Format("'{0}' operation success.", operationName)
						)
					);
				},
				async (message) => View("AdminPage", await GenerateAdminPageModelAsync(message))
			);
		}

		public CategoriesController(
			CategoriesManager categories,
			Performer<CategoriesController> performer)
		{
			_categories = categories;
			_performer = performer;
		}

		[HttpGet("base")]
		[AllowAnonymous]
		public async Task<JsonResult> GetBaseCategories()
		{
			return Json(
				await _categories.GetCategoriesOnParentAsync(null),
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
			);
		}

		[HttpGet("category/{parentId}/children")]
		[AllowAnonymous]
		public async Task<JsonResult> GetChildren(
			[FromRoute(Name = "parentId")] int parentId)
		{
			return Json(
				await _categories.GetCategoriesOnParentAsync(parentId),
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
			);
		}

		[HttpPost("action/switch")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> SwitchPopularity(
			[FromForm(Name = "categoryId")] int categoryId)
		{
			return PerformActionAsync(() => _categories.SwitchPopularityAsync(categoryId), "switch popularity");
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Create(
			[FromForm(Name = "categoryName")] string newCategoryName,
			[FromForm(Name = "parentId")] int parentId)
		{
			return PerformActionAsync(
				() => _categories.CreateCategoryAsync(parentId == 0 ? null : parentId, newCategoryName),
				"create"
			);
		}

		[HttpPost("action/rename")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Rename(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "categoryName")] string newName)
		{
			return PerformActionAsync(() => _categories.RenameCategoryAsync(categoryId, newName), "rename");
		}

		[HttpPost("action/move")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Rename(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "parentId")] int parentId)
		{
			return PerformActionAsync(() => _categories.MoveCategoryAsync(categoryId, parentId), "move");
		}

		[HttpPost("action/salvage")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Salvage(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "parentId")] int destinationId)
		{
			return PerformActionAsync(() => _categories.SalvageCategoryAsync(categoryId, destinationId), "salvage");
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Delete(
			[FromForm(Name = "categoryId")] int categoryId)
		{
			return PerformActionAsync(() => _categories.DeleteCategoryAsync(categoryId), "delete");
		}

		public async Task<IActionResult> Index()
		{
			return View("AdminPage", await GenerateAdminPageModelAsync("..."));
		}
	}
}