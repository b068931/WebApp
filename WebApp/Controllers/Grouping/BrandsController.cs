using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using WebApp.Database.Models.Grouping;
using WebApp.ProjectConfiguration.Constants;
using WebApp.Services.Actions;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Exceptions;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Grouping
{
	[Route("/brands")]
	[Authorize(Policy = "CriticalSiteContentPolicy")]
	public class BrandsController : Controller
	{
		private readonly BrandsManager _brands;
		private readonly IMemoryCache _cache;

		private readonly Performer<BrandsController> _performer;
		private readonly JsonSerializerOptions _jsonOptions;

		private async Task<ResultWithErrorVM<List<BrandModel>>> GetViewModelAsync(string error = "")
		{
			return new()
			{
				Result = await _brands.GetAllBrandsAsync(),
				Error = error
			};
		}
		private Task<IActionResult> PerformActionAsync(Func<Task> callback)
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					await callback();
					_cache.Remove(
						CacheKeys.AboutUsAllBrands
					);

					return View("AdminPage", await GetViewModelAsync());
				},
				async (message) => View("AdminPage", await GetViewModelAsync(message))
			);
		}

		public BrandsController(
			BrandsManager brands,
			IMemoryCache cache,
			Performer<BrandsController> performer)
		{
			_brands = brands;
			_cache = cache;
			_performer = performer;
			_jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
		}

		[HttpGet("/brands/json")]
		public async Task<JsonResult> GetAll()
		{
			return Json(await _brands.GetAllBrandsAsync(), _jsonOptions);
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Create(
			[FromForm(Name = "brandName")] string newBrandName,
			[FromForm(Name = "brandImage")] IFormFile brandImage)
		{
			return PerformActionAsync(
				() => _brands.CreateBrandAsync(newBrandName, brandImage ??
					throw new UserInteractionException("Trying to create a brand with no image.")
				)
			);
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Update(
			[FromForm(Name = "brandId")] int idToUpdate,
			[FromForm(Name = "brandName")] string newName,
			[FromForm(Name = "brandImage")] IFormFile? brandImage)
		{
			return PerformActionAsync(
				async () =>
				{
					await _brands.UpdateBrandAsync(idToUpdate, newName, brandImage);
					if (brandImage != null)
					{
						_cache.Remove(
							CacheKeys.GenerateBrandImageCacheKey(
								await _brands.GetBrandImageIdByBrandIdAsync(idToUpdate)
							)
						);
					}
				}
			);
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Delete(
			[FromForm(Name = "brandId")] int idToDelete)
		{
			return PerformActionAsync(
				async () =>
				{
					int brandImageId = await _brands.GetBrandImageIdByBrandIdAsync(idToDelete);
					await _brands.DeleteBrandAsync(idToDelete);

					_cache.Remove(
						CacheKeys.GenerateBrandImageCacheKey(brandImageId)
					);
				}
			);
		}

		public async Task<IActionResult> Index()
		{
			return View("AdminPage", await GetViewModelAsync());
		}
	}
}
