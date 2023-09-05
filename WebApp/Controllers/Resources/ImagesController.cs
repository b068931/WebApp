using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers.Grouping;
using WebApp.Database;
using WebApp.Database.Entities.Grouping;
using WebApp.Database.Entities.Products;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Products;

namespace WebApp.Controllers.Resources
{
    [Route("/images")]
    public class ImagesController : Controller
    {
        private readonly ProductImagesManager _images;
        private readonly BrandsManager _brands;
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
            ProductImagesManager images,
            BrandsManager brands,
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
