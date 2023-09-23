using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

		private async Task SendPasswordResetMessage(
			EmailSender sender,
			string receiver,
			PasswordResetVM model)
		{
			MimeMessage passwordResetMessage = sender.GetMessage();

			passwordResetMessage.To.Add(new MailboxAddress("User to be restored", receiver));
			passwordResetMessage.Subject = "Відновлення пароля";
			passwordResetMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
			{
				Text = await RenderViewAsync(
					"_PasswordResetEmailMessage",
					model,
					true
				)
			};

			await sender.SendMessage(passwordResetMessage);
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
						ApplicationUser newUser = new()
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
							ModelState.AddModelError("PasswordRepeat", "Помилка створення вашого акаунта.");
						}
					}

					return View("Register", registerVM);
				},
				(message) => ModelState.AddModelError("PasswordRepeat", message.Message),
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

					ApplicationUser foundUser = await _users.FindByIdAsync(userId.ToString())
						?? throw new UserInteractionException("Такий користувач не існує.");

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

		[AllowAnonymous]
		[HttpGet("reset/password")]
		public IActionResult ForgotPassword(
			[FromQuery(Name = "return")] string? returnUrl)
		{
			return View("ForgotPassword", new ForgotPasswordVM()
			{
				ReturnUrl = returnUrl
			});
		}

		[AllowAnonymous]
		[HttpPost("reset/password")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> ForgotPassword(
			ForgotPasswordVM forgotVM,
			[FromServices] EmailSender sender)
		{
			return _performer.PerformActionAsync(
				async () =>
				{
					if(ModelState.IsValid)
					{
						ApplicationUser foundUser =
							await _users.FindByEmailAsync(forgotVM.Email)
								?? throw new UserInteractionException("Такого акаунта не існує.");

						if (!await _users.IsEmailConfirmedAsync(foundUser))
							throw new UserInteractionException("На жаль, ви не підтвердили вашу пошту, а тому відновити акаунт неможливо.");

						string encodedToken = HttpUtility.UrlEncode(
							await _users.GeneratePasswordResetTokenAsync(foundUser)
						);

						await SendPasswordResetMessage(
							sender,
							foundUser.Email,
							new PasswordResetVM()
							{
								SiteBaseAddress = $"{Request.Scheme}://{Request.Host}",
								UserId = foundUser.Id,
								Token = encodedToken,
								ReturnUrl = forgotVM.ReturnUrl
							}
						);

						ModelState.AddModelError("Password", "Відновіть пароль, щоб увійти у ваш акаунт.");
						return View("Login", new LoginVM()
						{
							ReturnUrl = forgotVM.ReturnUrl
						});
					}

					return View("ForgotPassword", forgotVM);
				},
				(message) => ModelState.AddModelError(string.Empty, message.Message),
				() =>
				{
					ModelState.AddModelError(string.Empty, "Помилка виконання запиту на відновлення пароля.");
					return View("ForgotPassword", forgotVM);
				}
			);
		}

		[AllowAnonymous]
		[HttpGet("reset/newpassword")]
		public IActionResult ResetPassword(
			[FromQuery(Name = "token")] string resetToken,
			[FromQuery(Name = "user")] int userId,
			[FromQuery(Name = "return")] string returnUrl)
		{
			return View("ResetPassword", new PasswordResetFinishVM()
			{
				ReturnUrl = returnUrl,
				UserId = userId,
				Token = resetToken
			});
		}

		[AllowAnonymous]
		[HttpPost("reset/newpassword")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> ResetPassword(
			PasswordResetFinishVM resetVM)
		{
			return _performer.PerformActionAsync(
				async () =>
				{
					if(ModelState.IsValid)
					{
						ApplicationUser foundUser = await _users.FindByIdAsync(resetVM.UserId.ToString())
							?? throw new UserInteractionException("Вказаний користувач не існує.");

						var result = 
							await _users.ResetPasswordAsync(foundUser, resetVM.Token, resetVM.Password);

						if(result.Succeeded)
						{
							return View("Login", new LoginVM()
							{
								UserName = foundUser.UserName,
								ReturnUrl = resetVM.ReturnUrl
							});
						}
						else
						{
							ModelState.AddModelError("PasswordRepeat", "Помилка зміни пароля вашого акаунта.");
						}
					}

					return View("ResetPassword", new PasswordResetFinishVM()
					{
						ReturnUrl = resetVM.ReturnUrl,
						UserId = resetVM.UserId,
						Token = resetVM.Token
					});
				},
				(message) => ModelState.AddModelError("PasswordRepeat", message.Message),
				() =>
				{
					ModelState.AddModelError(string.Empty, "Помилка зміни пароля вашого акаунта.");
					return View("ResetPassword", new PasswordResetFinishVM()
					{
						ReturnUrl = resetVM.ReturnUrl,
						UserId = resetVM.UserId,
						Token = resetVM.Token
					});
				}
			);
		}
	}
}
