using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers.Grouping;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Interfaces;

namespace WebApp.Controllers.Products
{
	[Route("/images")]
	public class ProductImagesController : Controller
	{
		private readonly IProductImagesManager _images;
		private readonly ILogger<CategoriesController> _logger;

		private IActionResult PerformAction(Func<IActionResult> action)
		{
			try
			{
				return action();
			}
			catch(ProductInteractionException ex)
			{
				return BadRequest(ex.Message);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "ProductsImagesController error.");
				return StatusCode(StatusCodes.Status500InternalServerError, "Oops.");
			}
		}
		private FileResult GetImageFileResult(int imageToReturn)
		{
			Image foundImage = _images.FindImage(imageToReturn);
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
		public IActionResult GetImage(
			[FromRoute(Name = "imageId")] int imageToReturn)
		{
			return PerformAction(() => GetImageFileResult(imageToReturn));
		}
	}
}
