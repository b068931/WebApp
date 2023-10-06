namespace WebApp.ProjectConfiguration.Constants
{
    public static class CacheKeys
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
