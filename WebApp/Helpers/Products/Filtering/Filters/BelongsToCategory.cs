using System.Runtime.Serialization;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
	public class BelongsToCategory : IFilter<Product>
	{
		private readonly int _categoryId;
		public BelongsToCategory(int categoryId) => _categoryId = categoryId;

		public IEnumerable<Product> Apply(IEnumerable<Product> request)
			=> request.Where(e => e.CategoryId == _categoryId);

		public static IFilter<Product> CreateInstance(string value)
			=> new BelongsToCategory(int.Parse(value));
	}
}
