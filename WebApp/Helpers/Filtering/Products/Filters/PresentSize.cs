using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;
using WebApp.Helpers.Filtering;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class PresentSize : IFilter<Product>
	{
		private readonly List<int> _sizesIds;
		public PresentSize(List<int> sizesIds) => _sizesIds = sizesIds;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => e.Stocks.Any(e => _sizesIds.Contains(e.SizeId)));

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new PresentSize(
				value.Select(
					e => int.Parse(
						e ?? throw new ArgumentNullException("Null argument passed to sizess filter")
					)
				).ToList()
			);
	}
}
