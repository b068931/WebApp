using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.Database.Entities.Auth;

namespace WebApp.Controllers.Auth
{
	[Route("/auth")]
	public class Authentication : Controller
	{
		private readonly UserManager<ApplicationUser> _users;
		public Authentication(
			UserManager<ApplicationUser> users)
		{
			_users = users;
		}

		[AllowAnonymous]
		[HttpGet("login")]
		public IActionResult Login(
			[FromQuery(Name = "return")] string returnUrl)
		{
			return Content("Hello");
		}
	}
}
