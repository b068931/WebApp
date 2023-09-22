using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class BelongsToUser : IFilter<Product>
	{
		private readonly int _userId;
		public BelongsToUser(int userId) => _userId = userId;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => e.ProductOwnerId == _userId);

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new BelongsToUser(int.Parse(value.ToString()));
	}
}
