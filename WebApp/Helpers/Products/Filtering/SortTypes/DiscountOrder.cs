﻿using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.SortTypes
{
	public class DiscountOrder : IOrdering<Product>
	{
		private int _maxId;
		private int _maxDiscount;
		private GenericComparer<int> _sortDirection;
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

		public static IOrdering<Product> CreateInstance(int maxId, string value, bool isReversed)
			=> new DiscountOrder(
				maxId,
				int.Parse(value),
				new GenericComparer<int>(isReversed)
			);
	}
}