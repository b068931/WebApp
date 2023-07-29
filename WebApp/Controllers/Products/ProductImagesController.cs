using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers.Grouping;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers;
using WebApp.Services.Interfaces;

namespace WebApp.Controllers.Products
{
    [Route("/images")]
	public class ProductImagesController : Controller
	{
		private readonly IProductImagesManager _images;
		private readonly ILogger<CategoriesController> _logger;

		private async Task<IActionResult> PerformAction(Func<Task<IActionResult>> action)
		{
			try
			{
				return await action();
			}
			catch(UserInteractionException ex)
			{
				return BadRequest(ex.Message);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "ProductsImagesController error.");
				return StatusCode(StatusCodes.Status500InternalServerError, "Oops.");
			}
		}
		private async Task<IActionResult> GetImageFileResult(int imageToReturn)
		{
			Image foundImage = await _images.FindImage(imageToReturn);
			return new FileStreamResult(new MemoryStream(foundImage.Data), foundImage.ContentType);
		}

		public ProductImagesController(
			IProductImagesManager images,
			ILogger<CategoriesController> logger)
		{
			_images = images;
			_logger = logger;
		}

		[HttpGet("image/{imageId}")]
		public async Task<IActionResult> GetImage(
			[FromRoute(Name = "imageId")] int imageToReturn)
		{
			return await PerformAction(() => GetImageFileResult(imageToReturn));
		}
	}
}
