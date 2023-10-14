using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Database.Models.Stocks;
using WebApp.Services.Actions;
using WebApp.Services.Database.Products;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Products
{
    [Route("/sizes")]
	[Authorize(Policy = "CriticalSiteContentPolicy")]
	public class SizesController : Controller
	{
		private readonly SizesManager _sizes;
		private readonly Performer<SizesController> _performer;

		private async Task<ResultWithErrorVM<List<SizeModel>>> GetViewModel(string error = "")
		{
			return new()
			{
				Result = await _sizes.GetAllSizesAsync(),
				Error = error
			};
		}
		private Task<IActionResult> PerformActionAsync(Func<Task> callback)
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					await callback();
					return View("AdminPage", await GetViewModel());
				},
				async (message) => View("AdminPage", await GetViewModel(message))
			);
		}

		public SizesController(
			SizesManager sizes,
			Performer<SizesController> performer)
		{
			_sizes = sizes;
			_performer = performer;
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Create(
			[FromForm(Name = "sizeName")] string sizeName)
		{
			return PerformActionAsync(() => _sizes.CreateSizeAsync(sizeName));
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Update(
			[FromForm(Name = "sizeId")] int id,
			[FromForm(Name = "sizeName")] string sizeName)
		{
			return PerformActionAsync(() => _sizes.UpdateSizeAsync(id, sizeName));
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Delete(
			[FromForm(Name = "sizeId")] int id)
		{
			return PerformActionAsync(() => _sizes.DeleteSizeAsync(id));
		}

		public async Task<IActionResult> Index()
		{
			return View("AdminPage", await GetViewModel());
		}
	}
}
