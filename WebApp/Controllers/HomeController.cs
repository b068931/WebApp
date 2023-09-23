using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Services.Database.Grouping;
using WebApp.ViewModels.Other;

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

		[AllowAnonymous]
		public IActionResult Index()
		{
			return View("MainPage");
		}

		[HttpGet("/aboutus")]
		[AllowAnonymous]
		public async Task<IActionResult> AboutUs()
		{
			return View("AboutUs", new AboutUsVM()
			{
				PopularCategories = await _categories.GetPopularCategoriesAsync(),
				Brands = await _brands.GetAllBrandsAsync()
			});
		}

		[HttpGet("/admin")]
		[Authorize(Policy = "CriticalSiteContentPolicy")]
		public IActionResult AdminPanel()
		{
			return View("AdminPanel");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		[AllowAnonymous]
		[Route("/error")]
		public IActionResult Error()
		{
			return View();
		}
	}
}
