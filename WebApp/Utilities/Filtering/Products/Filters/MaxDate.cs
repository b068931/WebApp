﻿using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class MaxDate : IFilter<Product>
	{
		private readonly DateOnly _maxDate;
		public MaxDate(DateOnly maxDate) => _maxDate = maxDate;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => e.Created <= _maxDate);

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new MaxDate(DateOnly.Parse(value.ToString()));
	}
}
