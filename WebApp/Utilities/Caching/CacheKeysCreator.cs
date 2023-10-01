namespace WebApp.Utilities.Caching
{
	public static class CacheKeysCreator
	{
		public static string GenerateCategoryCacheKey(int categoryId)
		{
			return $"category-{categoryId}";
		}
		public static string GenerateBrandImageCacheKey(int imageId)
		{
			return $"brand-image-{imageId}";
		}
	}
}
