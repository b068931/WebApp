using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Resources
{
	[Route("/authorized")]
	public class AuthorizedStaticFilesController : Controller
	{
		private IWebHostEnvironment _environment;
		public AuthorizedStaticFilesController(IWebHostEnvironment environment) => 
			_environment = environment;

		[HttpGet("js/{filename}")]
		public IActionResult PublicAuthorizedJavascript(
			[FromRoute(Name = "filename")] string fileName)
		{
			return PhysicalFile(
				Path.Combine(_environment.ContentRootPath, "AuthorizedContent", "js", fileName),
				"text/javascript"
			);
		}

		[HttpGet("admin/js/{filename}")]
		[Authorize(Roles = "Admin")]
		public IActionResult AdminAuthorizedJavascript(
			[FromRoute(Name = "filename")] string fileName)
		{
			return PhysicalFile(
				Path.Combine(_environment.ContentRootPath, "AuthorizedContent", "js", "admin", fileName),
				"text/javascript"
			);
		}
	}
}
