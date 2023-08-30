using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities;
using WebApp.Helpers.Filtering;
using WebApp.Helpers.Exceptions;

namespace WebApp.Helpers.Products.Filtering
{
    public class ProductFiltersFactory
    {
        private Dictionary<string, Func<StringValues, IFilter<Product>>> _factories;
		public ProductFiltersFactory(
            Dictionary<string, Func<StringValues, IFilter<Product>>> factories)
        {
            _factories = factories;
		}

		public List<IFilter<Product>> ParseFilters(Dictionary<string, StringValues> filtersInformation)
        {
            List<IFilter<Product>> filtersList = new List<IFilter<Product>>();
            foreach (var filterInformation in filtersInformation)
            {
                try
                {
					Func<StringValues, IFilter<Product>>? factoryMethod = null;
					if (_factories.TryGetValue(filterInformation.Key, out factoryMethod))
                    {
						filtersList.Add(
                            factoryMethod.Invoke(filterInformation.Value)
					    );
					}
                }
                catch (Exception)
                {
                    throw new UserInteractionException("Unable to create filters for your request.");
                }
            }

            return filtersList;
        }
    }
}
