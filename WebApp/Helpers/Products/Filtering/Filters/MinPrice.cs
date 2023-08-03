using System.Runtime.Serialization;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class MinPrice : IFilter<Product>
    {
        private readonly int _minPrice;
        public MinPrice(int minPrice) => _minPrice = minPrice;

        public IEnumerable<Product> Apply(IEnumerable<Product> request)
            => request.Where(e => e.Price >= _minPrice);

        public static IFilter<Product> CreateInstance(string value)
            => new MinPrice(int.Parse(value));
	}
}
