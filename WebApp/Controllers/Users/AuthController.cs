using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web;
using WebApp.Controllers.Abstract;
using WebApp.Database.Entities.Auth;
using WebApp.Services.Actions;
using WebApp.Utilities.Exceptions;
using WebApp.ViewModels.Auth;

namespace WebApp.Controllers.Users
{
    [Route("/auth")]
	public class AuthController : ExtendedController
	{
		private readonly UserManager<ApplicationUser> _users;
		private readonly SignInManager<ApplicationUser> _signIn;
		private readonly Performer<AuthController> _performer;

		private IActionResult GetReturnUrlResult(string? returnUrl)
		{
			if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return Redirect("/");
			}
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
		public async Task<IActionResult> Login(
			[FromQuery(Name = "return")] string? returnUrl)
		{
			return View("Login", new LoginVM()
			{
				ReturnUrl = returnUrl,
				ExternalSchemes = await _signIn.GetExternalAuthenticationSchemesAsync()
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
						loginVM.UserName, loginVM.Password, loginVM.RememberMe, false
					);

				if (result.Succeeded)
				{
					return GetReturnUrlResult(loginVM.ReturnUrl);
				}
				else if (result.IsNotAllowed)
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

		[AllowAnonymous]
		[HttpPost("external")]
		[ValidateAntiForgeryToken]
		public IActionResult LoginExternal(
			[FromForm(Name = "provider")] string? provider)
		{
			if (provider == null)
				return Redirect("/auth/login");

			var externalSignInProperties =
				_signIn.ConfigureExternalAuthenticationProperties(
					provider,
					"/auth/external/finish"
				);

			return new ChallengeResult(provider, externalSignInProperties);
		}

		[AllowAnonymous]
		[HttpGet("external/finish")]
		public Task<IActionResult> LoginExternalFinish()
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					var externalAccountInformation = await _signIn.GetExternalLoginInfoAsync() ?? 
						throw new UserInteractionException(
							"Помилка завантаження інформації зовнішнього акаунта."
						);

					var externalSignInResult =
						await _signIn.ExternalLoginSignInAsync(
							externalAccountInformation.LoginProvider,
							externalAccountInformation.ProviderKey,
							true, true
						);

					if(externalSignInResult.Succeeded)
					{
						return Redirect("/");
					}
					else
					{
						string? email = externalAccountInformation
							.Principal
							.FindFirstValue(ClaimTypes.Email) ??
								throw new UserInteractionException(
									"Помилка пошуку інформації зовнішнього акаунта."
								);

						string? name = externalAccountInformation
							.Principal
							.FindFirstValue(ClaimTypes.Name) ?? email;

						if (await _users.FindByEmailAsync(email) != null)
							throw new UserInteractionException("Користувач з вашим email вже існує.");

						ApplicationUser newUser = new()
						{
							UserName = name,
							Email = email,
							EmailConfirmed = true
						};

						await _users.CreateAsync(newUser);

						await _users.AddToRoleAsync(newUser, "user");
						await _users.AddLoginAsync(newUser, externalAccountInformation);

						await _signIn.SignInAsync(newUser, true);
						return Redirect("/");
					}
				},
				async (message) =>
				{
					LoginVM loginVM = new()
					{
						ReturnUrl = null,
						ExternalSchemes = await _signIn.GetExternalAuthenticationSchemesAsync()
					};

					ModelState.AddModelError("Password", message);
					return View("Login", loginVM);
				}
			);
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

							await sender.ControllerSendEmail(
								this,
								registerVM.Email,
								"Підтвердження вашого email",
								"_EmailConfirmationEmailMessage",
								new EmailConfirmationVM()
								{
									SiteBaseAddress = SiteBaseAddress,
									UserId = newUser.Id,
									Token = encodedToken,
									ReturnUrl = registerVM.ReturnUrl
								},
								"User to be confirmed"
							);

							ModelState.AddModelError("UserName", "Підтвердіть ваш email для того, щоб увійти в акаунт.");
							return View("Login", new LoginVM()
							{
								UserName = registerVM.UserName,
								ReturnUrl = registerVM.ReturnUrl,
								ExternalSchemes = await _signIn.GetExternalAuthenticationSchemesAsync()
							});
						}
						else
						{
							ModelState.AddModelError("PasswordRepeat", "Неможливо створити ваш акаунт.");
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
					if (ModelState.IsValid)
					{
						ApplicationUser foundUser =
							await _users.FindByEmailAsync(forgotVM.Email)
								?? throw new UserInteractionException("Такого акаунта не існує.");

						if (foundUser.PasswordHash == null)
							throw new UserInteractionException("Відновлення пароля для вашого акаунта не підтримується, бо ми його не зберігаємо.");

						if (!await _users.IsEmailConfirmedAsync(foundUser))
							throw new UserInteractionException("На жаль, ви не підтвердили вашу пошту, а тому відновити акаунт неможливо.");

						string encodedToken = HttpUtility.UrlEncode(
							await _users.GeneratePasswordResetTokenAsync(foundUser)
						);

						await sender.ControllerSendEmail(
							this,
							foundUser.Email ?? throw new ArgumentNullException(nameof(foundUser.Email), "User with no email"),
							"Відновлення пароля",
							"_PasswordResetEmailMessage",
							new PasswordResetVM()
							{
								SiteBaseAddress = SiteBaseAddress,
								UserId = foundUser.Id,
								Token = encodedToken,
								ReturnUrl = forgotVM.ReturnUrl
							},
							"User to be restored"
						);

						ModelState.AddModelError("Password", "Відновіть пароль, щоб увійти у ваш акаунт.");
						return View("Login", new LoginVM()
						{
							ReturnUrl = forgotVM.ReturnUrl,
							ExternalSchemes = await _signIn.GetExternalAuthenticationSchemesAsync()
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
					if (ModelState.IsValid)
					{
						ApplicationUser foundUser = await _users.FindByIdAsync(resetVM.UserId.ToString())
							?? throw new UserInteractionException("Вказаний користувач не існує.");

						var result =
							await _users.ResetPasswordAsync(foundUser, resetVM.Token, resetVM.Password);

						if (result.Succeeded)
						{
							return View("Login", new LoginVM()
							{
								UserName = foundUser.UserName ?? throw new ArgumentNullException(nameof(foundUser.UserName), "User with no name."),
								ReturnUrl = resetVM.ReturnUrl,
								ExternalSchemes = await _signIn.GetExternalAuthenticationSchemesAsync()
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

		[AllowAnonymous]
		[HttpGet("noentry")]
		public IActionResult AccessDenied(
			[FromQuery(Name = "return")] string? triedPath)
		{
			if (!string.IsNullOrEmpty(triedPath) && Url.IsLocalUrl(triedPath))
			{
				return View("AccessDenied", triedPath);
			}

			return Redirect("/");
		}
	}
}
