using Microsoft.Extensions.Primitives;
using System.Globalization;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.OrderTypes
{
	public class PriceOrder : IOrdering<Product>
	{
		private int _maxId;
		private decimal _maxPrice;
		private GenericComparer<decimal> _sortDirection;
		public PriceOrder(int maxId, decimal maxPrice, GenericComparer<decimal> direction)
		{
			_maxId = maxId;
			_maxPrice = maxPrice;
			_sortDirection = direction;
		}

		public IQueryable<Product> Apply(IQueryable<Product> request)
		{
			request = _sortDirection.ApplyOrdering(e => e.TruePrice, request);
			if (_sortDirection.IsReversed)
			{
				request = request
					.Where(e =>
						(e.TruePrice < _maxPrice) ||
							(e.TruePrice == _maxPrice && e.Id > _maxId)
					);
			}
			else
			{
				request = request
					.Where(e =>
						(e.TruePrice > _maxPrice) ||
							(e.TruePrice == _maxPrice && e.Id > _maxId)
					);
			}

			return request;
		}

		public static IOrdering<Product> CreateInstance(int maxId, StringValues value, bool isReversed)
			=> new PriceOrder(
				maxId,
				decimal.Parse(value.ToString(), CultureInfo.InvariantCulture),
				new GenericComparer<decimal>(isReversed)
			);
	}
}
