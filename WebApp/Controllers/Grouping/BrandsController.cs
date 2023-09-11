using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Database.Models;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Grouping
{
	[Route("/brands")]
	[Authorize(Policy = "CriticalSiteContentPolicy")]
	public class BrandsController : Controller
	{
		private readonly BrandsManager _brands;
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
					return View("AdminPage", await GetViewModelAsync());
				},
				async (message) => View("AdminPage", await GetViewModelAsync(message))
			);
		}

		public BrandsController(
			BrandsManager brands,
			Performer<BrandsController> performer)
		{
			_brands = brands;
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
			[FromForm(Name = "brandId")] int idToRename,
			[FromForm(Name = "brandName")] string newName,
			[FromForm(Name = "brandImage")] IFormFile? brandImage)
		{
			return PerformActionAsync(
				() => _brands.UpdateBrandAsync(idToRename, newName, brandImage)
			);
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Delete(
			[FromForm(Name = "brandId")] int idToDelete)
		{
			return PerformActionAsync(
				() => _brands.DeleteBrandAsync(idToDelete)
			);
		}

		public async Task<IActionResult> Index()
		{
			return View("AdminPage", await GetViewModelAsync());
		}
	}
}
