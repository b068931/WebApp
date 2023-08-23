using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers.Grouping;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers;
using WebApp.Services.Interfaces.Grouping;
using WebApp.Services.Interfaces.Products;

namespace WebApp.Controllers
{
    [Route("/images")]
    public class ImagesController : Controller
    {
        private readonly IProductImagesManager _images;
        private readonly IBrandsManager _brands;
        private readonly ILogger<CategoriesController> _logger;

        private async Task<IActionResult> PerformAction(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (UserInteractionException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProductsImagesController error.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Oops.");
            }
        }
        
        private async Task<IActionResult> GetProductImageFileResult(int imageToReturn)
        {
            ProductImage foundImage = await _images.FindImage(imageToReturn);
            return new FileStreamResult(new MemoryStream(foundImage.Data), foundImage.ContentType);
        }
        private async Task<IActionResult> GetBrandImageFileResult(int imageToReturn)
        {
            BrandImage foundImage = await _brands.GetBrandImage(imageToReturn);
			return new FileStreamResult(new MemoryStream(foundImage.Data), foundImage.ContentType);
		}

        public ImagesController(
            IProductImagesManager images,
            IBrandsManager brands,
            ILogger<CategoriesController> logger)
        {
            _images = images;
            _brands = brands;
            _logger = logger;
        }

        [HttpGet("image/{imageId}")]
        public async Task<IActionResult> GetImage(
            [FromRoute(Name = "imageId")] int imageToReturn)
        {
            return await PerformAction(() => GetProductImageFileResult(imageToReturn));
        }

        [HttpGet("brandImage/{imageId}")]
        public async Task<IActionResult> GetBrandImage(
            [FromRoute(Name = "imageId")] int imageToReturn)
        {
            return await PerformAction(() => GetBrandImageFileResult(imageToReturn));
        }
    }
}
