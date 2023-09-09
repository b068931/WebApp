using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Resources
{
	[Route("/authorized")]
	public class AuthorizedStaticFilesController : Controller
	{
		private IWebHostEnvironment _environment;
		private ILogger<AuthorizedStaticFilesController> _logger;

		private IActionResult GetFile(string fileName, string contentType, params string[] path)
		{

			var absolutePath = Path.Combine(
				path
					.Prepend(_environment.ContentRootPath)
					.Append(fileName)
					.ToArray()
			);

			if (System.IO.File.Exists(absolutePath))
			{
				return PhysicalFile(
					absolutePath,
					contentType
				);
			}
			else
			{
				_logger.LogWarning(
					string.Format("Non-existent static file with name '{0}' was accessed.", fileName)
				);

				return NotFound();
			}
		}

		public AuthorizedStaticFilesController(
			IWebHostEnvironment environment,
			ILogger<AuthorizedStaticFilesController> logger)
		{
			_environment = environment;
			_logger = logger;
		}

		[HttpGet("js/{filename}")]
		public IActionResult PublicAuthorizedJavascript(
			[FromRoute(Name = "filename")] string fileName)
		{
			return GetFile(fileName, "text/javascript", "AuthorizedContent", "js");
		}

		[HttpGet("admin/js/{filename}")]
		[Authorize(Policy = "CriticalSiteContentPolicy")]
		public IActionResult AdminAuthorizedJavascript(
			[FromRoute(Name = "filename")] string fileName)
		{
			return GetFile(fileName, "text/javascript", "AuthorizedContent", "js", "admin");
		}
	}
}
