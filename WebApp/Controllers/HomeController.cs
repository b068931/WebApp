using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Database.Models.Grouping;
using WebApp.ProjectConfiguration.Constants;
using WebApp.Services.Database.Grouping;
using WebApp.ViewModels.Categories;
using WebApp.ViewModels.Other;

namespace WebApp.Controllers
{
	public class HomeController : Controller
	{
		private readonly IMemoryCache _cache;

		private readonly CategoriesManager _categories;
		private readonly BrandsManager _brands;

		private static void ConfigureAboutUsPageCachedEntry(ICacheEntry cacheEntry)
		{
			cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(10);
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

			cacheEntry.Size = 2;
			cacheEntry.Priority = CacheItemPriority.Normal;
		}

		public HomeController(
			IMemoryCache cache,
			CategoriesManager categories,
			BrandsManager brands)
		{
			_cache = cache;
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
			List<CategoryVM> popularCategories = await _cache.GetOrCreateAsync(
				CacheKeys.AboutUsPopularCategories,
				cacheEntry =>
				{
					ConfigureAboutUsPageCachedEntry(cacheEntry);
					return _categories.GetPopularCategoriesAsync();
				}
			) ?? throw new ArgumentNullException(nameof(popularCategories), "Unexpected null value for popular categories");

			List<BrandModel> allBrands = await _cache.GetOrCreateAsync(
				CacheKeys.AboutUsAllBrands,
				cacheEntry =>
				{
					ConfigureAboutUsPageCachedEntry(cacheEntry);
					return _brands.GetAllBrandsAsync();
				}
			) ?? throw new ArgumentNullException(nameof(allBrands), "Unexpected null value for all brands list");

			return View("AboutUs", new AboutUsVM()
			{
				PopularCategories = popularCategories,
				Brands = allBrands
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
