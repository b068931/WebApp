using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class SelectConcreteProducts : IFilter<Product>
	{
		private readonly List<int> _selectedIds;
		public SelectConcreteProducts(List<int> selectedIds) => _selectedIds = selectedIds;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => _selectedIds.Contains(e.Id));

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new SelectConcreteProducts(
				value.Select(e =>
					int.Parse(
						e ?? throw new ArgumentNullException(nameof(value), "Null argument passed to SelectConcreteProducts filter")
					)
				).ToList()
			);
	}
}
