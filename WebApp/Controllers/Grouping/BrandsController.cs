using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WebApp.Database.Models;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers.Grouping
{
	[Route("/brands")]
	[Authorize(Roles = "admin")]
	public class BrandsController : Controller
	{
		private readonly BrandsManager _brands;
		private readonly Performer<BrandsController> _performer;
		private readonly JsonSerializerOptions _jsonOptions;

		private ResultWithErrorVM<List<Brand>> GetViewModel(string error = "")
		{
			return new()
			{
				Result = _brands.GetAllBrands(),
				Error = error
			};
		}
		private IActionResult PerformAction(Action callback)
		{
			return _performer.PerformActionMessage(
				() =>
				{
					callback();
					return View("AdminPage", GetViewModel());
				},
				(message) => View("AdminPage", GetViewModel(message))
			);
		}

		public BrandsController(
			ILogger<BrandsController> logger, 
			BrandsManager brands)
		{
			_brands = brands;
			_performer = new Performer<BrandsController>(logger);
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
			return View("AdminPage", GetViewModel());
		}
	}
}
