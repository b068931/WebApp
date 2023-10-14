using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Database.Models.Stocks;
using WebApp.Services.Actions;
using WebApp.Services.Database.Products;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Products
{
    [Route("/colours")]
	[Authorize(Policy = "CriticalSiteContentPolicy")]
	public class ColoursController : Controller
	{
		private readonly ColoursManager _colours;
		private readonly Performer<ColoursController> _performer;

		private async Task<ResultWithErrorVM<List<ColourModel>>> GetViewModel(string error = "")
		{
			return new()
			{
				Result = await _colours.GetAllColoursAsync(),
				Error = error
			};
		}
		private Task<IActionResult> PerformActionAsync(Func<Task> action)
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					await action();
					return View("AdminPage", await GetViewModel());
				},
				async (message) => View("AdminPage", await GetViewModel(message))
			);
		}

		public ColoursController(
			ColoursManager colours,
			Performer<ColoursController> performer)
		{
			_colours = colours;
			_performer = performer;
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Create(
			[FromForm(Name = "colourName")] string name,
			[FromForm(Name = "colourHex")] string colour)
		{
			return PerformActionAsync(() => _colours.CreateColourAsync(name, colour));
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Update(
			[FromForm(Name = "colourId")] int id,
			[FromForm(Name = "colourName")] string name,
			[FromForm(Name = "colourHex")] string colour)
		{
			return PerformActionAsync(() => _colours.UpdateColourAsync(id, name, colour));
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Delete(
			[FromForm(Name = "colourId")] int id)
		{
			return PerformActionAsync(() => _colours.DeleteColourAsync(id));
		}

		public async Task<IActionResult> Index()
		{
			return View("AdminPage", await GetViewModel());
		}
	}
}
