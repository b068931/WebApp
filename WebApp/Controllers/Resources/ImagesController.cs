using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Database.Models.Images;
using WebApp.ProjectConfiguration.Constants;
using WebApp.Services.Database.Grouping;
using WebApp.Utilities.Other;

namespace WebApp.Controllers.Resources
{
    [Route("/images")]
	[AllowAnonymous]
	public class ImagesController : Controller
	{
		private readonly BrandsManager _brands;

		private readonly IMemoryCache _cache;
		private readonly Performer<ImagesController> _performer;

		private static void ConfigureImageCacheEntry(ICacheEntry cacheEntry)
		{
			cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(10);
			cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

			cacheEntry.Size = 2;
			cacheEntry.Priority = CacheItemPriority.Normal;
		}

		private Task<IActionResult> PerformAction(Func<Task<IActionResult>> action)
		{
			return _performer.PerformActionMessageAsync(
				() => action(),
				(message) => BadRequest(message)
			);
		}
		private async Task<IActionResult> GetBrandImageFileResultAsync(int imageToReturn)
		{
			BrandImageModel brandImage = await _cache.GetOrCreateAsync(
				CacheKeys.GenerateBrandImageCacheKey(imageToReturn),
				cacheEntry =>
				{
					ConfigureImageCacheEntry(cacheEntry);
					return _brands.GetBrandImageAsync(imageToReturn);
				}
			) ?? throw new NullReferenceException("Unexpected null reference from brand image cache.");

			return new FileStreamResult(new MemoryStream(brandImage.ImageData), brandImage.ContentType);
		}

		public ImagesController(
			BrandsManager brands,
			IMemoryCache cache,
			Performer<ImagesController> performer)
		{
			_brands = brands;
			_cache = cache;
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
