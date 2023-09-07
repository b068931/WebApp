using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Exceptions;

namespace WebApp.Controllers.Grouping
{
	[Route("/brands")]
	public class BrandsController : Controller
	{
		private readonly ILogger<BrandsController> _logger;
		private readonly BrandsManager _brands;
		private readonly JsonSerializerOptions _jsonOptions;

		private IActionResult PerformAction(Action callback)
		{
			try
			{
				callback();
			}
			catch (UserInteractionException ex)
			{
				_logger.LogWarning(ex, "BrandsController incorrect action.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "BrandsController error.");
			}

			return Redirect("/brands");
		}

		public BrandsController(ILogger<BrandsController> logger, BrandsManager brands)
		{
			_logger = logger;
			_brands = brands;
			_jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
		}

		[HttpGet("/brands/json")]
		public JsonResult GetAll()
		{
			return Json(_brands.GetAllBrands(), _jsonOptions);
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			[FromForm(Name = "brandName")] string newBrandName,
			[FromForm(Name = "brandImage")] IFormFile brandImage)
		{
			return PerformAction(
				() => _brands.CreateBrand(newBrandName, brandImage ??
					throw new UserInteractionException("Trying to create a brand with no image.")
				)
			);
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public IActionResult Update(
			[FromForm(Name = "brandId")] int idToRename,
			[FromForm(Name = "brandName")] string newName,
			[FromForm(Name = "brandImage")] IFormFile? brandImage)
		{
			return PerformAction(() => _brands.UpdateBrand(idToRename, newName, brandImage));
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "brandId")] int idToDelete)
		{
			return PerformAction(() => _brands.DeleteBrand(idToDelete));
		}

		public IActionResult Index()
		{
			return View("AdminPage", _brands.GetAllBrands());
		}
	}
}
