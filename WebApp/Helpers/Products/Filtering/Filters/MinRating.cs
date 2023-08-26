using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
	public class MinRating : IFilter<Product>
	{
		private readonly int _minRating;
		public MinRating(int minRating) => _minRating = minRating;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => 
				e.TrueRating >= _minRating
			);

		public static IFilter<Product> CreateInstance(StringValues value) 
			=> new MinRating(int.Parse(value.ToString()));
	}
}
