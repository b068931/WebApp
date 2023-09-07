using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class MinPrice : IFilter<Product>
	{
		private readonly int _minPrice;
		public MinPrice(int minPrice) => _minPrice = minPrice;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => e.Price >= _minPrice);

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new MinPrice(int.Parse(value.ToString()));
	}
}
