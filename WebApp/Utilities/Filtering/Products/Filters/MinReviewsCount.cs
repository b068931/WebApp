using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class MinReviewsCount : IFilter<Product>
	{
		private readonly int _minReviewsCount;
		public MinReviewsCount(int minReviewsCount)
			=> _minReviewsCount = minReviewsCount;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e =>
			e.RatingsCount >= _minReviewsCount
		);

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new MinReviewsCount(int.Parse(value.ToString()));
	}
}
