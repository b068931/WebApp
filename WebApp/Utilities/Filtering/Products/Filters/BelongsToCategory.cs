using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class BelongsToCategory : IFilter<Product>
	{
		private readonly List<int> _categoriesIds;
		public BelongsToCategory(List<int> categoriesIds) => _categoriesIds = categoriesIds;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => _categoriesIds.Contains(e.CategoryId));

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new BelongsToCategory(
				value.Select(e =>
					int.Parse(
						e ?? throw new ArgumentNullException(nameof(value), "Null argument passed to categories filter")
					)
				).ToList()
			);
	}
}
