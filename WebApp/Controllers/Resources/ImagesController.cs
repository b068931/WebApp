using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Other;

namespace WebApp.Controllers.Resources
{
	[Route("/images")]
	[AllowAnonymous]
	public class ImagesController : Controller
	{
		private readonly BrandsManager _brands;
		private readonly Performer<ImagesController> _performer;

		private Task<IActionResult> PerformAction(Func<Task<IActionResult>> action)
		{
			return _performer.PerformActionMessageAsync(
				() => action(),
				(message) => BadRequest(message)
			);
		}
		private Task<IActionResult> GetBrandImageFileResultAsync(int imageToReturn)
		{
			return _brands.GetBrandImageAsync(imageToReturn)
				.ContinueWith<IActionResult>(next =>
					new FileStreamResult(new MemoryStream(next.Result.Data), next.Result.ContentType)
				);
		}

		public ImagesController(
			BrandsManager brands,
			Performer<ImagesController> performer)
		{
			_brands = brands;
			_performer = performer;
		}

		[HttpGet("brandImage/{imageId}")]
		public Task<IActionResult> GetBrandImage(
			[FromRoute(Name = "imageId")] int imageToReturn)
		{
			return PerformAction(() => GetBrandImageFileResultAsync(imageToReturn));
		}
	}
}
