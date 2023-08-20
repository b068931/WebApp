using System.ComponentModel;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.OrderTypes
{
	public class ViewsOrder : IOrdering<Product>
	{
		private int _maxId;
		private int _maxViews;
		private GenericComparer<int> _sortDirection;
		public ViewsOrder(int maxId, int maxViews, GenericComparer<int> direction)
		{
			_maxId = maxId;
			_maxViews = maxViews;
			_sortDirection = direction;
		}

		public IQueryable<Product> Apply(IQueryable<Product> request)
		{
			request = _sortDirection.ApplyOrdering(e => e.ViewsCount, request);
			if(_sortDirection.IsReversed)
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

		public static IOrdering<Product> CreateInstance(int maxId, string value, bool isReversed)
			=> new ViewsOrder(
				maxId, 
				int.Parse(value),
				new GenericComparer<int>(isReversed)
			);
	}
}
