using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.OrderTypes
{
	public class DiscountOrder : IOrdering<Product>
	{
		private readonly int _maxId;
		private readonly int _maxDiscount;
		private readonly GenericComparer<int> _sortDirection;
		public DiscountOrder(int maxId, int maxDiscount, GenericComparer<int> direction)
		{
			_maxId = maxId;
			_maxDiscount = maxDiscount;
			_sortDirection = direction;
		}

		public IQueryable<Product> Apply(IQueryable<Product> request)
		{
			request = _sortDirection.ApplyOrdering(e => e.Discount, request);
			if (_sortDirection.IsReversed)
			{
				request = request
					.Where(e =>
						(e.Discount < _maxDiscount) ||
							(e.Discount == _maxDiscount && e.Id > _maxId)
					);
			}
			else
			{
				request = request
					.Where(e =>
						(e.Discount > _maxDiscount) ||
							(e.Discount == _maxDiscount && e.Id > _maxId)
					);
			}

			return request;
		}

		public static IOrdering<Product> CreateInstance(int maxId, StringValues value, bool isReversed)
			=> new DiscountOrder(
				maxId,
				int.Parse(value.ToString()),
				new GenericComparer<int>(isReversed)
			);
	}
}
