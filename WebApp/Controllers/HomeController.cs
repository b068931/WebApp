using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApp.Database;
using WebApp.Database.Models;
using WebApp.Services.Interfaces.Grouping;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class HomeController : Controller
	{
		private readonly ICategoriesManager _categories;
		private readonly IBrandsManager _brands;

		public HomeController(
			ICategoriesManager categories,
			IBrandsManager brands)
		{
			_categories = categories;
			_brands = brands;
		}

		public IActionResult Index()
		{
            return View("MainPage");
		}

		[HttpGet("/aboutus")]
		public IActionResult AboutUs()
		{
			return View("AboutUs", new AboutUsVM()
			{
				PopularCategories = _categories.GetPopularCategories(),
				Brands = _brands.GetAllBrands()
			});
		}

		[HttpGet("/admin")]
		public IActionResult AdminPanel()
		{
			return View("AdminPanel");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
