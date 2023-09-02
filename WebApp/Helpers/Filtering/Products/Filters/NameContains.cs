using Microsoft.Extensions.Primitives;
using System.Runtime.Serialization;
using WebApp.Database.Entities.Products;
using WebApp.Helpers.Filtering;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class NameContains : IFilter<Product>
    {
        private readonly string _substring;
        public NameContains(string substring) => _substring = substring;

        public IQueryable<Product> Apply(IQueryable<Product> request)
            => request.Where(e => e.Name.Contains(_substring));

        public static IFilter<Product> CreateInstance(StringValues value) 
            => new NameContains(value.ToString());
	}
}
