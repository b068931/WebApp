using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Services.Implementation.Grouping;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class HomeController : Controller
	{
		private readonly CategoriesManager _categories;
		private readonly BrandsManager _brands;

		public HomeController(
			CategoriesManager categories,
			BrandsManager brands)
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
