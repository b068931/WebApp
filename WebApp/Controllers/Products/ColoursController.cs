using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Database.Entities.Products;
using WebApp.Database.Models;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Products
{
	[Route("/colours")]
	[Authorize(Roles = "admin")]
	public class ColoursController : Controller
	{
		private readonly ColoursManager _colours;
		private readonly Performer<ColoursController> _performer;
		
		private ResultWithErrorVM<List<Database.Models.Colour>> GetViewModel(string error = "")
		{
			return new()
			{
				Result = _colours.GetAllColours(),
				Error = error
			};
		}
		private IActionResult PerformAction(Action action)
		{
			return _performer.PerformActionMessage(
				() =>
				{
					action();
					return View("AdminPage", GetViewModel());
				},
				(message) => View("AdminPage", GetViewModel(message))
			);
		}

		public ColoursController(
			ColoursManager colours, 
			ILogger<ColoursController> logger)
		{
			_colours = colours;
			_performer = new Performer<ColoursController>(logger);
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			[FromForm(Name = "colourName")] string name,
			[FromForm(Name = "colourHex")] string colour)
		{
			return PerformAction(() => _colours.CreateColour(name, colour));
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public IActionResult Update(
			[FromForm(Name = "colourId")] int id,
			[FromForm(Name = "colourName")] string name,
			[FromForm(Name = "colourHex")] string colour)
		{
			return PerformAction(() => _colours.UpdateColour(id, name, colour));
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "colourId")] int id)
		{
			return PerformAction(() => _colours.DeleteColour(id));
		}

		public IActionResult Index()
		{
			return View("AdminPage", GetViewModel());
		}
	}
}
