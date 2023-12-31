﻿using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class MinDate : IFilter<Product>
	{
		private readonly DateOnly _minDate;
		public MinDate(DateOnly minDate) => _minDate = minDate;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request.Where(e => e.Created >= _minDate);

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new MinDate(DateOnly.Parse(value.ToString()));
	}
}
