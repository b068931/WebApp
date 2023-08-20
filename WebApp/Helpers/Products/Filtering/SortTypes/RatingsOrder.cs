using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.OrderTypes
{
	public class RatingsOrder : IOrdering<Product>
	{
		private int _maxId;
		private int _maxRating;
		private GenericComparer<int> _sortDirection;
		public RatingsOrder(int maxId, int maxRating, GenericComparer<int> direction)
		{
			_maxId = maxId;
			_maxRating = maxRating;
			_sortDirection = direction;
		}

		public IQueryable<Product> Apply(IQueryable<Product> request)
		{
			request = _sortDirection.ApplyOrdering(e => e.Stars, request);
			if(_sortDirection.IsReversed)
			{
				request = request
					.Where(e =>
						(e.Stars < _maxRating) ||
							(e.Stars == _maxRating && e.Id > _maxId)
					);
			}
			else
			{
				request = request
					.Where(e =>
						(e.Stars > _maxRating) ||
							(e.Stars == _maxRating && e.Id > _maxId)
					);
			}

			return request;
		}

		public static IOrdering<Product> CreateInstance(int maxId, string value, bool isReversed)
			=> new RatingsOrder(
				maxId, 
				int.Parse(value), 
				new GenericComparer<int>(isReversed)
			);
	}
}
