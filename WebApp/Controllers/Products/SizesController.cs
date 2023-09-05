using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Implementation.Products;

namespace WebApp.Controllers.Products
{
    [Route("/sizes")]
	public class SizesController : Controller
	{
		private readonly SizesManager _sizes;
		private readonly ILogger<SizesController> _logger;
		private IActionResult PerformAction(Action callback)
		{
			try
			{
				callback();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SizesController error.");
			}

			return Redirect("/sizes");
		}

		public SizesController(SizesManager sizes, ILogger<SizesController> logger)
		{
			_sizes = sizes;
			_logger = logger;
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			[FromForm(Name = "sizeName")] string sizeName)
		{
			return PerformAction(() => _sizes.CreateSize(sizeName));
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public IActionResult Update(
			[FromForm(Name = "sizeId")] int id,
			[FromForm(Name = "sizeName")] string sizeName)
		{
			return PerformAction(() => _sizes.UpdateSize(id, sizeName));
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "sizeId")] int id)
		{
			return PerformAction(() => _sizes.DeleteSize(id));
		}

		public IActionResult Index()
		{
			return View("AdminPage", _sizes.GetAllSizes());
		}
	}
}
