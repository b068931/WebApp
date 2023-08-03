using System.Runtime.Serialization;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class BelongsToBrand : IFilter<Product>
    {
        private readonly int _brandId;
        public BelongsToBrand(int brandId) => _brandId = brandId;

        public IEnumerable<Product> Apply(IEnumerable<Product> request)
            => request.Where(e => e.BrandId == _brandId);

        public static IFilter<Product> CreateInstance(string value)
            => new BelongsToBrand(int.Parse(value));
	}
}
