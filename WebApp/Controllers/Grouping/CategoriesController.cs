using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Caching;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Grouping
{
	[Route("/categories")]
	[Authorize(Policy = "CriticalSiteContentPolicy")]
	public class CategoriesController : Controller
	{
		private readonly CategoriesManager _categories;
		private readonly JsonSerializerSettings _jsonSettings;

		private readonly IMemoryCache _cache;
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
							$"'{operationName}' operation success."
						)
					);
				},
				async (message) => View("AdminPage", await GenerateAdminPageModelAsync(message))
			);
		}

		private static void ConfigureCacheEntry(ICacheEntry cacheEntry)
		{
			cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(10);
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

			cacheEntry.Size = 1;
			cacheEntry.Priority = CacheItemPriority.High;
		}

		public CategoriesController(
			CategoriesManager categories,
			IMemoryCache cache,
			Performer<CategoriesController> performer)
		{
			_categories = categories;
			_cache = cache;
			_performer = performer;
			_jsonSettings = new JsonSerializerSettings()
			{
				ContractResolver = new DefaultContractResolver()
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				}
			};
		}

		[HttpGet("base")]
		[AllowAnonymous]
		public async Task<IActionResult> GetBaseCategories()
		{
			string? baseCategoriesJson = await _cache.GetOrCreateAsync(
				CacheKeys.GenerateCategoryCacheKey(0),
				async cacheEntry =>
				{
					ConfigureCacheEntry(cacheEntry);
					return JsonConvert.SerializeObject(
						await _categories.GetCategoriesOnParentAsync(null),
						_jsonSettings
					);
				}
			);

			return Content(baseCategoriesJson ?? "", "application/json");
		}

		[HttpGet("category/{parentId}/children")]
		[AllowAnonymous]
		public async Task<IActionResult> GetChildren(
			[FromRoute(Name = "parentId")] int parentId)
		{
			string? categoryChildrenJson = await _cache.GetOrCreateAsync(
				CacheKeys.GenerateCategoryCacheKey(parentId),
				async cacheEntry =>
				{
					ConfigureCacheEntry(cacheEntry);
					return JsonConvert.SerializeObject(
						await _categories.GetCategoriesOnParentAsync(parentId),
						_jsonSettings
					);
				}
			);

			return Content(categoryChildrenJson ?? "", "application/json");
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
				async () =>
				{
					await _categories.CreateCategoryAsync(parentId == 0 ? null : parentId, newCategoryName);
					_cache.Remove(
						CacheKeys.GenerateCategoryCacheKey(parentId)
					);
				},
				"create"
			);
		}

		[HttpPost("action/rename")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Rename(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "categoryName")] string newName)
		{
			return PerformActionAsync(
				async () =>
				{
					await _categories.RenameCategoryAsync(categoryId, newName);
					_cache.Remove(
						CacheKeys.GenerateCategoryCacheKey(await _categories.GetParentId(categoryId) ?? 0)
					);
				}, 
				"rename"
			);
		}

		[HttpPost("action/move")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Move(
			[FromForm(Name = "categoryId")] int categoryId,
			[FromForm(Name = "parentId")] int parentId)
		{
			return PerformActionAsync(
				async () =>
				{
					await _categories.MoveCategoryAsync(categoryId, parentId);

					_cache.Remove(
						CacheKeys.GenerateCategoryCacheKey(await _categories.GetParentId(categoryId) ?? 0)
					);

					_cache.Remove(
						CacheKeys.GenerateCategoryCacheKey(parentId)
					);
				},
				"move"
			);
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
			return PerformActionAsync(
				async () =>
				{
					await _categories.DeleteCategoryAsync(categoryId);

					//Deletion is a recursive operation. Most likely, this won't invalidate all caches.
					//Either way, they can't be accessed so no one cares.
					_cache.Remove(
						CacheKeys.GenerateCategoryCacheKey(await _categories.GetParentId(categoryId) ?? 0)
					);
				},
				"delete"
			);
		}

		public async Task<IActionResult> Index()
		{
			return View(
				"AdminPage", 
				await GenerateAdminPageModelAsync("NOTE: Your actions invalidate cached categories.")
			);
		}
	}
}