﻿using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApp.Services.Interfaces.Grouping;

namespace WebApp.Controllers.Grouping
{
    [Route("/brands")]
    public class BrandsController : Controller
    {
        private readonly ILogger<BrandsController> _logger;
        private readonly IBrandsManager _brands;

		private IActionResult PerformAction(Action callback)
		{
			try
			{
				callback();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "BrandsController error.");
			}

			return Redirect("/brands");
		}

		public BrandsController(ILogger<BrandsController> logger, IBrandsManager brands)
		{
			_logger = logger;
			_brands = brands;
		}

        [HttpGet("/brands/json")]
        public JsonResult GetAll()
        {
            return Json(
                _brands.GetAllBrands(),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );
        }

        [HttpPost("action/create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [FromForm(Name = "brandName")] string newBrandName,
            [FromForm(Name = "brandImage")] IFormFile brandImage)
        {
            return PerformAction(() => _brands.CreateBrand(newBrandName, brandImage));
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
