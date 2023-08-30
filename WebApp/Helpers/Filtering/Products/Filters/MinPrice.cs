using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities;
using WebApp.Helpers.Filtering;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class MinPrice : IFilter<Product>
    {
        private readonly int _minPrice;
        public MinPrice(int minPrice) => _minPrice = minPrice;

        public IQueryable<Product> Apply(IQueryable<Product> request)
            => request.Where(e => e.Price >= _minPrice);

        public static IFilter<Product> CreateInstance(StringValues value)
            => new MinPrice(int.Parse(value.ToString()));
	}
}
