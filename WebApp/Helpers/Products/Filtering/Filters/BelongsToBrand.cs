using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class BelongsToBrand : IFilter<Product>
    {
        private readonly List<int> _brandsIds;
        public BelongsToBrand(List<int> brandsIds) => _brandsIds = brandsIds;

        public IQueryable<Product> Apply(IQueryable<Product> request)
            => request.Where(e => _brandsIds.Contains(e.BrandId ?? 0));

        public static IFilter<Product> CreateInstance(StringValues value)
            => new BelongsToBrand(
                value.Select(
                    e => int.Parse(
                        e ?? throw new ArgumentNullException("Null argument passed to brands filter")
                    )
                ).ToList()
            );
	}
}
