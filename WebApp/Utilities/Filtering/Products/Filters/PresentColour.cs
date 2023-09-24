using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class PresentColour : IFilter<Product>
	{
		private readonly List<int> _coloursIds;
		public PresentColour(List<int> coloursIds) => _coloursIds = coloursIds;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => e.Stocks.Any(e => _coloursIds.Contains(e.ColourId)));

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new PresentColour(
				value.Select(
					e => int.Parse(
						e ?? throw new ArgumentNullException(nameof(value), "Null argument passed to colours filter")
					)
				).ToList()
			);
	}
}
