using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Other;

namespace WebApp.Controllers.Resources
{
	[Route("/images")]
	[AllowAnonymous]
	public class ImagesController : Controller
	{
		private readonly ProductImagesManager _images;
		private readonly BrandsManager _brands;
		private readonly Performer<ImagesController> _performer;

		private Task<IActionResult> PerformAction(Func<Task<IActionResult>> action)
		{
			return _performer.PerformActionMessageAsync(
				() => action(),
				(message) => BadRequest(message)
			);
		}

		private Task<IActionResult> GetProductImageFileResultAsync(int imageToReturn)
		{
			return _images.FindImageAsync(imageToReturn)
				.ContinueWith<IActionResult>(next =>
					new FileStreamResult(new MemoryStream(next.Result.Data), next.Result.ContentType)
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
			ProductImagesManager images,
			BrandsManager brands,
			Performer<ImagesController> performer)
		{
			_images = images;
			_brands = brands;
			_performer = performer;
		}

		[HttpGet("image/{imageId}")]
		public Task<IActionResult> GetImage(
			[FromRoute(Name = "imageId")] int imageToReturn)
		{
			return PerformAction(() => GetProductImageFileResultAsync(imageToReturn));
		}

		[HttpGet("brandImage/{imageId}")]
		public Task<IActionResult> GetBrandImage(
			[FromRoute(Name = "imageId")] int imageToReturn)
		{
			return PerformAction(() => GetBrandImageFileResultAsync(imageToReturn));
		}
	}
}
