using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
	public class MinRating : IFilter<Product>
	{
		private readonly int _minRating;
		public MinRating(int minRating) => _minRating = minRating;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => 
				(e.StarsCount / ((e.RatingsCount == 0) ? 1 : e.RatingsCount)) >= _minRating
			);

		public static IFilter<Product> CreateInstance(string value) 
			=> new MinRating(int.Parse(value));
	}
}
