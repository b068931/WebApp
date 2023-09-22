using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Controllers.Abstract;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Filtering.Products.Filters;
using WebApp.Utilities.Filtering.Products.OrderTypes;
using WebApp.ViewModels.Account;

namespace WebApp.Controllers.Auth
{
	[Route("/account")]
	[Authorize(Policy = "PublicContentPolicy")]
	public class AccountController : ExtendedController
	{
		private const int RecentlyViewedProductsPageSize = 12;
		private readonly ProductsManager _products;

		public AccountController(
			ProductsManager products)
		{
			_products = products;
		}

		[HttpGet("products")]
		public async Task<IActionResult> MyProducts()
		{
			return View("MyProducts", new MyProductsVM()
			{
				ProductsList = await _products.SearchAsync(
					new List<Utilities.Filtering.IFilter<Database.Entities.Products.Product>>()
					{
						new BelongsToUser(GetUserId())
					},
					new IdOrder(0),
					ProductsManager.MaxProductsPerUser,
					false
				)
			});
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			return View("AccountPage", new AccountPageVM()
			{
				Name = User.FindFirstValue(ClaimTypes.Name),
				Email = User.FindFirstValue(ClaimTypes.Email),
				RecentlyViewedProducts = await _products.SearchAsync(
					new List<Utilities.Filtering.IFilter<Database.Entities.Products.Product>>()
					{
						new ViewedByUser(GetUserId())
					},
					new IdOrder(0),
					RecentlyViewedProductsPageSize,
					false
				)
			});
		}
	}
}
