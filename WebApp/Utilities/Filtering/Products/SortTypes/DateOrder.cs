using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.OrderTypes
{
	public class DateOrder : IOrdering<Product>
	{
		private readonly int _maxId;
		private readonly DateOnly _maxDate;
		private readonly GenericComparer<DateOnly> _sortDirection;
		public DateOrder(int maxId, DateOnly maxDate, GenericComparer<DateOnly> direction)
		{
			_maxId = maxId;
			_maxDate = maxDate;
			_sortDirection = direction;
		}

		public IQueryable<Product> Apply(IQueryable<Product> request)
		{
			request = _sortDirection.ApplyOrdering(e => e.Created, request);
			if (_sortDirection.IsReversed)
			{
				request = request.Where(e =>
					(e.Created < _maxDate) ||
						(e.Created == _maxDate && e.Id > _maxId)
				);
			}
			else
			{
				request = request.Where(e =>
					(e.Created > _maxDate) ||
						(e.Created == _maxDate && e.Id > _maxId)
				);
			}

			return request;
		}

		public static IOrdering<Product> CreateInstance(int maxId, StringValues value, bool isReversed)
			=> new DateOrder(
				maxId,
				DateOnly.Parse(value.ToString()),
				new GenericComparer<DateOnly>(isReversed)
			);
	}
}
