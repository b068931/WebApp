using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class NameContains : IFilter<Product>
	{
		private readonly string _substring;
		public NameContains(string substring) => _substring = substring;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => e.Name.Contains(_substring));

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new NameContains(value.ToString());
	}
}
