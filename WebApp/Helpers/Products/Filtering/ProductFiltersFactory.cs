using WebApp.Database.Entities;
using WebApp.Helpers.Products.Filtering.Filters;

namespace WebApp.Helpers.Products.Filtering
{
    public class ProductFiltersFactory
    {
        private Dictionary<string, Func<string, IFilter<Product>>> _factories;
		public ProductFiltersFactory(Dictionary<string, Func<string, IFilter<Product>>> factories)
            => _factories = factories;

		public List<IFilter<Product>> ParseFilters(Dictionary<string, string> filtersInformation)
        {
            List<IFilter<Product>> filtersList = new List<IFilter<Product>>();
            foreach (var filterInformation in filtersInformation)
            {
                try
                {
                    filtersList.Add(
                        _factories[filterInformation.Key].Invoke(filterInformation.Value)
                    );
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
