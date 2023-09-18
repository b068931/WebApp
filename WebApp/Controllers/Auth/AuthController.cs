using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;
using NuGet.Common;
using System.Text;
using System.Web;
using WebApp.Controllers.Abstract;
using WebApp.Database.Entities.Auth;
using WebApp.Services.Actions;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Auth;

namespace WebApp.Controllers.Auth
{
	[Route("/auth")]
	public class AuthController : ExtendedController
	{
		private readonly UserManager<ApplicationUser> _users;
		private readonly SignInManager<ApplicationUser> _signIn;
		private readonly Performer<AuthController> _performer;

		private async Task SendEmailConfirmationMessage(
			EmailSender sender, 
			string receiver,
			EmailConfirmationVM model)
		{
			MimeMessage emailConfirmationMessage = sender.GetMessage();
			
			emailConfirmationMessage.To.Add(new MailboxAddress("User to be confirmed", receiver));
			emailConfirmationMessage.Subject = "Підтвердження вашого email";
			emailConfirmationMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
			{
				Text = await RenderViewAsync(
					"_EmailConfirmationEmailMessage",
					model,
					true
				)
			};

			await sender.SendMessage(emailConfirmationMessage);
		}

		public AuthController(
			UserManager<ApplicationUser> users,
			SignInManager<ApplicationUser> signIn,
			Performer<AuthController> perfromer)
		{
			_users = users;
			_signIn = signIn;
			_performer = perfromer;
		}

		[AllowAnonymous]
		[HttpGet("login")]
		public IActionResult Login(
			[FromQuery(Name = "return")] string? returnUrl)
		{
			return View("Login", new LoginVM()
			{
				ReturnUrl = returnUrl
			});
		}

		[AllowAnonymous]
		[HttpPost("login")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginVM loginVM)
		{
			if (ModelState.IsValid)
			{
				var result =
					await _signIn.PasswordSignInAsync(
						loginVM.UserName, loginVM.Password, true, false
					);

				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(loginVM.ReturnUrl) && Url.IsLocalUrl(loginVM.ReturnUrl))
					{
						return Redirect(loginVM.ReturnUrl);
					}
					else
					{
						return Redirect("/");
					}
				}
				else if(result.IsNotAllowed)
				{
					ModelState.AddModelError("Password", "Спочатку підтвердіть ваш email.");
				}
				else
				{
					ModelState.AddModelError("Password", "Ви вказали неправильний пароль та/або логін акаунта.");
				}
			}

			return View("Login", loginVM);
		}

		[HttpPost("logout")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signIn.SignOutAsync();
			return Redirect("/");
		}

		[AllowAnonymous]
		[HttpGet("register")]
		public IActionResult Register(
			[FromQuery(Name = "return")] string? returnUrl)
		{
			return View("Register", new RegisterVM()
			{
				ReturnUrl = returnUrl
			});
		}

		[AllowAnonymous]
		[HttpPost("register")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Register(
			RegisterVM registerVM,
			[FromServices] EmailSender sender)
		{
			return _performer.PerformActionAsync(
				async () =>
				{
					if (ModelState.IsValid)
					{
						ApplicationUser newUser = new ApplicationUser()
						{
							UserName = registerVM.UserName,
							Email = registerVM.Email
						};

						var result = await _users.CreateAsync(newUser, registerVM.Password);
						if (result.Succeeded)
						{
							string encodedToken = HttpUtility.UrlEncode(
								await _users.GenerateEmailConfirmationTokenAsync(newUser)
							);

							await SendEmailConfirmationMessage(
								sender,
								registerVM.Email,
								new EmailConfirmationVM()
								{
									SiteBaseAddress = $"{Request.Scheme}://{Request.Host}",
									UserId = newUser.Id,
									Token = encodedToken,
									ReturnUrl = registerVM.ReturnUrl
								}
							);

							ModelState.AddModelError("UserName", "Підтвердіть ваш email для того, щоб увійти в акаунт.");
							return View("Login", new LoginVM()
							{
								UserName = registerVM.UserName,
								ReturnUrl = registerVM.ReturnUrl
							});
						}
						else
						{
							ModelState.AddModelError("Password", "Помилка створення вашого акаунта.");
						}
					}

					return View("Register", registerVM);
				},
				(message) => ModelState.AddModelError("Password", message.Message),
				() =>
				{
					ModelState.AddModelError(string.Empty, "Помилка створення вашого акаунта.");
					return View("Register", registerVM);
				}
			);
		}

		[AllowAnonymous]
		[HttpGet("confirmation/email")]
		public Task<IActionResult> ConfirmEmail(
			[FromQuery(Name = "token")] string confirmationToken,
			[FromQuery(Name = "user")] int userId,
			[FromQuery(Name = "return")] string? returnUrl)
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					if (string.IsNullOrEmpty(confirmationToken))
						throw new UserInteractionException("Ви не вказали токен для підтвердження.");

					ApplicationUser foundUser = await _users.FindByIdAsync(userId.ToString());
					if (foundUser == null)
						throw new UserInteractionException("Такий користувач не існує.");

					var result = await _users.ConfirmEmailAsync(foundUser, confirmationToken);
					if (!result.Succeeded)
						throw new UserInteractionException("Неможливо підтвердити користувача.");

					await _users.AddToRoleAsync(foundUser, "user");
					return View("EmailConfirmed", new EmailConfirmationFinishVM()
					{
						ReturnUrl = returnUrl
					});
				},
				(message) => 
					View("EmailConfirmed", new EmailConfirmationFinishVM()
					{
						Error = message
					})
			);
		}
	}
}
