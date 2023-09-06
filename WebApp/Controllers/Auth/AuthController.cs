using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Database.Entities.Auth;
using WebApp.ViewModels.Auth;

namespace WebApp.Controllers.Auth
{
	[Route("/auth")]
	public class AuthController : Controller
	{
		private readonly UserManager<ApplicationUser> _users;
		private readonly SignInManager<ApplicationUser> _signIn;

		public AuthController(
			UserManager<ApplicationUser> users, 
			SignInManager<ApplicationUser> signIn)
		{
			_users = users;
			_signIn = signIn;
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
			if(ModelState.IsValid)
			{
				var result =
					await _signIn.PasswordSignInAsync(
						loginVM.Email, loginVM.Password, true, false
					);

				if(result.Succeeded)
				{
					if(!string.IsNullOrEmpty(loginVM.ReturnUrl) && Url.IsLocalUrl(loginVM.ReturnUrl))
					{
						return Redirect(loginVM.ReturnUrl);
					}
					else
					{
						return Redirect("/");
					}
				}
				else
				{
					ModelState.AddModelError("Password", "Ви вказали неправильний пароль та/або електронну пошту.");
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
		public IActionResult Register(RegisterVM registerVM)
		{
			return View("Register", registerVM);
		}
	}
}
