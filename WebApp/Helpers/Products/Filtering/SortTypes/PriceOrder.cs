﻿using WebApp.Database.Entities;
using WebApp.Helpers.Products.Filtering.OrderTypes;

namespace WebApp.Helpers.Products.Filtering.SortTypes
{
	public class PriceOrder : IOrdering<Product>
	{
		private int _maxId;
		private int _maxPrice;
		private GenericComparer<decimal> _sortDirection;
		public PriceOrder(int maxId, int maxPrice, GenericComparer<decimal> direction)
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

		public static IOrdering<Product> CreateInstance(int maxId, string value, bool isReversed)
			=> new PriceOrder(
				maxId,
				int.Parse(value),
				new GenericComparer<decimal>(isReversed)
			);
	}
}