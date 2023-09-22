using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;

namespace WebApp.Utilities.Filtering.Products.Filters
{
	public class ViewedByUser : IFilter<Product>
	{
		private readonly int _userId;
		public ViewedByUser(int userId) => _userId = userId;

		public IQueryable<Product> Apply(IQueryable<Product> request)
			=> request
				.Include(e => e.ViewedByUsers)
				.Where(e => e.ViewedByUsers.Any(e => e.Id == _userId));

		public static IFilter<Product> CreateInstance(StringValues value)
			=> new ViewedByUser(int.Parse(value.ToString()));
	}
}
