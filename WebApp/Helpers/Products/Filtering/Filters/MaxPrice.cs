using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class MaxPrice : IFilter<Product>
    {
        private readonly int _maxPrice;
        public MaxPrice(int maxPrice) => _maxPrice = maxPrice;

        public IQueryable<Product> Apply(IQueryable<Product> request)
            => request.Where(e => e.Price <= _maxPrice);

        public static IFilter<Product> CreateInstance(StringValues value) 
            => new MaxPrice(int.Parse(value.ToString()));
	}
}
