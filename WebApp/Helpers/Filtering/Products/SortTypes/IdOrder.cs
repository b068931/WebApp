﻿using WebApp.Database.Entities.Products;
using WebApp.Helpers.Filtering;

namespace WebApp.Helpers.Products.Filtering.OrderTypes
{
    public class IdOrder : IOrdering<Product>
	{
		private int _maxId;
		public IdOrder(int maxId)
			=> _maxId = maxId;

		public IQueryable<Product> Apply(IQueryable<Product> request)
		   => request
				.OrderBy(e => e.Id)
				.Where(e => e.Id > _maxId);

		public static IOrdering<Product> CreateInstance(int maxId) 
			=> new IdOrder(maxId);
	}
}