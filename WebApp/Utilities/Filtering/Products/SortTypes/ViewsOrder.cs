using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.OrderTypes
{
	public class ViewsOrder : IOrdering<Product>
	{
		private readonly int _maxId;
		private readonly int _maxViews;
		private readonly GenericComparer<int> _sortDirection;
		public ViewsOrder(int maxId, int maxViews, GenericComparer<int> direction)
		{
			_maxId = maxId;
			_maxViews = maxViews;
			_sortDirection = direction;
		}

		public IQueryable<Product> Apply(IQueryable<Product> request)
		{
			request = _sortDirection.ApplyOrdering(e => e.ViewsCount, request);
			if (_sortDirection.IsReversed)
			{
				request = request
					.Where(e =>
						(e.ViewsCount < _maxViews) ||
							(e.ViewsCount == _maxViews && e.Id > _maxId)
					);
			}
			else
			{
				request = request
					.Where(e =>
						(e.ViewsCount > _maxViews) ||
							(e.ViewsCount == _maxViews && e.Id > _maxId)
					);
			}

			return request;
		}

		public static IOrdering<Product> CreateInstance(int maxId, StringValues value, bool isReversed)
			=> new ViewsOrder(
				maxId,
				int.Parse(value.ToString()),
				new GenericComparer<int>(isReversed)
			);
	}
}
