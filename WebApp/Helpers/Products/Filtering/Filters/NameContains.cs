using System.Runtime.Serialization;
using WebApp.Database.Entities;

namespace WebApp.Helpers.Products.Filtering.Filters
{
    public class NameContains : IFilter<Product>
    {
        private readonly string _substring;
        public NameContains(string substring) => _substring = substring;

        public IEnumerable<Product> Apply(IEnumerable<Product> request)
            => request.Where(e => e.Name.Contains(_substring));

        public static IFilter<Product> CreateInstance(string value) 
            => new NameContains(value);
	}
}
