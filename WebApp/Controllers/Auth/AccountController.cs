using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Controllers.Abstract;
using WebApp.Database.Entities.Auth;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Filtering.Products.Filters;
using WebApp.Utilities.Filtering.Products.OrderTypes;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Account;

namespace WebApp.Controllers.Auth
{
	[Route("/account")]
	[Authorize(Policy = "PublicContentPolicy")]
	public class AccountController : ExtendedController
	{
		private const int RecentlyViewedProductsPageSize = 12;

		private readonly ProductsManager _products;
		private readonly UserManager<ApplicationUser> _users;
		private readonly Performer<AccountController> _performer;

		public AccountController(
			ProductsManager products,
			UserManager<ApplicationUser> users,
			Performer<AccountController> performer)
		{
			_products = products;
			_users = users;
			_performer = performer;
		}

		[HttpGet("settings")]
		public IActionResult AccountSettings()
		{
			return View("AccountSettings");
		}

		[HttpPost("change/name")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeUserName(
			[FromForm] ChangeUserNameVM userNameVM)
		{
			if (ModelState.IsValid)
			{
				ApplicationUser foundUser = await _users.FindByIdAsync(GetUserId().ToString());

				foundUser.UserName = userNameVM.UserName;
				var result = await _users.UpdateAsync(foundUser);
				if (!result.Succeeded)
				{
					ModelState.AddModelError(string.Empty, "Не вдалося змінити ім'я користувача.");
				}
			}

			return View("AccountSettings");
		}

		[HttpPost("change/password")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeUserPassword(
			[FromForm] ChangePasswordVM passwordVM)
		{
			if(ModelState.IsValid)
			{
				ApplicationUser foundUser = await _users.FindByIdAsync(GetUserId().ToString());
				var result = await _users.ChangePasswordAsync(
					foundUser,
					passwordVM.OldPassword,
					passwordVM.NewPassword
				);

				if (!result.Succeeded)
				{
					ModelState.AddModelError(string.Empty, "Не вдалося змінити пароль.");
				}
			}

			return View("AccountSettings");
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
