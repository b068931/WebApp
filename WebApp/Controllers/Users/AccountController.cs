﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web;
using WebApp.Controllers.Abstract;
using WebApp.Database.Entities.Auth;
using WebApp.Database.Models.Products;
using WebApp.Extensions;
using WebApp.ProjectConfiguration.Constants;
using WebApp.Services.Actions;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Filtering.Products.Filters;
using WebApp.Utilities.Filtering.Products.OrderTypes;
using WebApp.ViewModels.Account;

namespace WebApp.Controllers.Users
{
	[Route("/account")]
	[Authorize(Policy = "PublicContentPolicy")]
	public class AccountController : ExtendedController
	{
		private const int RecentlyViewedProductsPageSize = 12;

		private readonly Performer<AccountController> _performer;
		private readonly ProductsManager _products;

		private readonly UserManager<ApplicationUser> _users;
		private readonly SignInManager<ApplicationUser> _signIn;

		public AccountController(
			Performer<AccountController> performer,
			ProductsManager products,
			UserManager<ApplicationUser> users,
			SignInManager<ApplicationUser> signIn)
		{
			_performer = performer;
			_products = products;

			_users = users;
			_signIn = signIn;
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
				ApplicationUser foundUser =
					await _users.FindByIdAsync(GetUserId().ToString())
						?? throw new ArgumentException($"User with id {GetUserId()} does not exist.");

				foundUser.UserName = userNameVM.UserName;
				var result = await _users.UpdateAsync(foundUser);
				if (!result.Succeeded)
				{
					ModelState.AddModelError(string.Empty, "Не вдалося змінити ім'я користувача.");
				}
				else
				{
					return View("AccountSettings", "Ім'я успішно змінено. Перезайдіть у свій акаунт.");
				}
			}

			return View("AccountSettings");
		}

		[HttpPost("change/password")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeUserPassword(
			[FromForm] ChangePasswordVM passwordVM)
		{
			if (ModelState.IsValid)
			{
				if (User.FindFirstValue(ApplicationClaimTypes.HasPasswordAuthentication) == bool.FalseString)
				{
					ModelState.AddModelError(string.Empty, "Для вашого акаунта неможливо змінити пароль.");
					return View("AccountSettings", "Пароль успішно змінено.");
				}

				ApplicationUser foundUser =
					await _users.FindByIdAsync(GetUserId().ToString())
						?? throw new ArgumentException($"User with id {GetUserId()} does not exist.");

				var result = await _users.ChangePasswordAsync(
					foundUser,
					passwordVM.OldPassword,
					passwordVM.NewPassword
				);

				if (!result.Succeeded)
				{
					ModelState.AddModelError(string.Empty, "Не вдалося змінити пароль.");
				}
				else
				{
					return View("AccountSettings", "Пароль успішно змінено.");
				}
			}

			return View("AccountSettings");
		}

		[HttpPost("change/email")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeUserEmail(
			[FromForm] ChangeEmailVM emailVM,
			[FromServices] EmailSender sender)
		{
			if (ModelState.IsValid)
			{
				ApplicationUser foundUser =
					await _users.FindByIdAsync(GetUserId().ToString())
						?? throw new ArgumentException($"User with id {GetUserId()} does not exist.");

				string encodedToken = HttpUtility.UrlEncode(
					await _users.GenerateChangeEmailTokenAsync(foundUser, emailVM.NewEmail)
				);

				await sender.ControllerSendEmail(
					this,
					emailVM.NewEmail,
					"Зміна email на новий",
					"_EmailChangeEmailMessage",
					new ChangeEmailMessageVM()
					{
						SiteBaseAddress = SiteBaseAddress,
						UserId = foundUser.Id,
						Token = encodedToken,
						NewEmail = emailVM.NewEmail
					},
					"New email account"
				);

				return View("AccountSettings", "Якщо вказана вами електронна поштова скринька існує, то на неї було надіслано повідомлення з підтвердженням.");
			}

			return View("AccountSettings");
		}

		[HttpGet("change/email")]
		public async Task<IActionResult> ChangeUserEmail(
			[FromQuery(Name = "token")] string emailChangeToken,
			[FromQuery(Name = "user")] int userId,
			[FromQuery(Name = "newemail")] string newEmail)
		{
			ApplicationUser? foundUser = await _users.FindByIdAsync(userId.ToString());
			if (foundUser == null)
				return View("EmailChanged", "Користувач не існує.");

			var result = await _users.ChangeEmailAsync(foundUser, newEmail, emailChangeToken);
			if (!result.Succeeded)
				return View("EmailChanged", "Неможливо підтвердити користувача");

			return View("EmailChanged");
		}

		[HttpPost("remove")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteMyAccount(
			[FromServices] EmailSender sender)
		{
			ApplicationUser foundUser =
				await _users.FindByIdAsync(GetUserId().ToString())
					?? throw new ArgumentException($"User with id {GetUserId()} does not exist.");

			string encodedToken = HttpUtility.UrlEncode(
				await _users.GenerateUserTokenAsync(
					foundUser,
					CustomTokenProviders.DeleteUserAccount,
					CustomTokenProviders.DeleteUserAccountPurpose
				)
			);

			await sender.ControllerSendEmail(
				this,
				foundUser.Email ?? throw new ArgumentException($"User {GetUserId()} with no email"),
				"Видалення вашого акаунта",
				"_DeleteMyAccountEmailMessage",
				new DeleteMyAccountEmailMessageVM()
				{
					SiteBaseAddress = SiteBaseAddress,
					UserId = foundUser.Id,
					Token = encodedToken,
					UserName = foundUser.UserName ?? foundUser.Email
				},
				"User to be deleted"
			);

			return View("AccountSettings", "На вашу пошту було надіслано повідомлення з підтвердженням видалення акаунта.");
		}

		[HttpGet("remove")]
		public async Task<IActionResult> DeleteMyAccount(
			[FromQuery(Name = "token")] string accountDeletionToken,
			[FromQuery(Name = "user")] int userId)
		{
			ApplicationUser? foundUser = await _users.FindByIdAsync(userId.ToString());
			if (foundUser == null)
				return View("AccountDeleted", "Такого користувача не існує.");

			bool result = await _users.VerifyUserTokenAsync(
				foundUser,
				CustomTokenProviders.DeleteUserAccount,
				CustomTokenProviders.DeleteUserAccountPurpose,
				accountDeletionToken
			);

			if (!result)
				return View("AccountDeleted", "Використано неправильний токен видалення акаунта.");

			return await _performer.PerformActionMessageAsync(
				async () =>
				{
					List<int> myProducts =
						await _products.GetProductsOfOwnerAsync(foundUser.Id);

					foreach (var product in myProducts)
						await _products.DeleteProductAsync(product);

					await _signIn.SignOutAsync();
					await _users.DeleteAsync(foundUser);

					return View("AccountDeleted");
				},
				(message) => View("AccountDeleted", message)
			);
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
			List<int> userClaimedViewedProducts =
				Request.Cookies.GetIntsList(CookieKeys.ViewedProducts);

			List<ProductPreview> recentlyViewedProducts;
			if (userClaimedViewedProducts.Count > 0)
			{
				recentlyViewedProducts = await _products.SearchAsync(
					new List<Utilities.Filtering.IFilter<Database.Entities.Products.Product>>()
					{
						new SelectConcreteProducts(
							userClaimedViewedProducts
								.TakeLast(RecentlyViewedProductsPageSize)
								.ToList()
						)
					},
					new IdOrder(0),
					RecentlyViewedProductsPageSize,
					false
				);
			}
			else
			{
				recentlyViewedProducts = await _products.SearchAsync(
					new List<Utilities.Filtering.IFilter<Database.Entities.Products.Product>>()
					{
						new ViewedByUser(GetUserId())
					},
					new IdOrder(0),
					RecentlyViewedProductsPageSize,
					false
				);

				Response.Cookies.SetIntsList(
					CookieKeys.ViewedProducts,
					recentlyViewedProducts
						.Select(e => e.Id)
						.ToList(),
					new CookieOptions()
					{
						Expires = DateTime.UtcNow.AddDays(CookieKeys.ViewedProductInformationCookieLifetimeDays),
						SameSite = SameSiteMode.Lax
					}
				);
			}

			return View("AccountPage", new AccountPageVM()
			{
				Name = User.FindFirstValue(ClaimTypes.Name) ?? "[NO_USER_NAME]",
				Email = User.FindFirstValue(ClaimTypes.Email) ?? "[NO_EMAIL]",
				CreationDateString = User.FindFirstValue(ApplicationClaimTypes.AccountCreationDate) ?? "[NO_ACCOUNT_CREATION_DATE]",
				RecentlyViewedProducts = recentlyViewedProducts
			});
		}
	}
}
