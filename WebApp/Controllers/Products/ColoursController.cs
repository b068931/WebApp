using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Interfaces.Products;

namespace WebApp.Controllers.Products
{
	[Route("/colours")]
	public class ColoursController : Controller
	{
		private readonly IColoursManager _colours;
		private readonly ILogger<ColoursController> _logger;
		private IActionResult PerformAction(Action callback)
		{
			try
			{
				callback();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ColoursController error.");
			}

			return Redirect("/colours");
		}

		public ColoursController(IColoursManager colours, ILogger<ColoursController> logger)
		{
			_colours = colours;
			_logger = logger;
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
			return View("AdminPage", _colours.GetAllColours());
		}
	}
}
