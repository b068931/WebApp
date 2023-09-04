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
		public AuthController(
			UserManager<ApplicationUser> users)
		{
			_users = users;
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
		public IActionResult Login(LoginVM loginVM)
		{
			return View("Login", loginVM);
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
		public IActionResult Register(RegisterVM registerVM)
		{
			return View("Register", registerVM);
		}
	}
}
