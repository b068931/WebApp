﻿using System.Linq.Expressions;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering
{
	public class GenericComparer<T> where T : IComparable<T>
	{
		private readonly bool _isReversed;
		public GenericComparer(bool isReversed)
			=> _isReversed = isReversed;

		public bool IsReversed { get { return _isReversed; } }
		public IOrderedQueryable<Product> ApplyOrdering(
			Expression<Func<Product, T>> selector,
			IQueryable<Product> request)
		{
			return (_isReversed ? request.OrderByDescending(selector) : request.OrderBy(selector))
				.ThenBy(e => e.Id);
		}
	}
}
