using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;
using WebApp.Utilities.Exceptions;

namespace WebApp.Utilities.Filtering.Products
{
	public class ProductFiltersFactory
	{
		private readonly Dictionary<string, Func<StringValues, IFilter<Product>>> _factories;
		public ProductFiltersFactory(
			Dictionary<string, Func<StringValues, IFilter<Product>>> factories)
		{
			_factories = factories;
		}

		public List<IFilter<Product>> ParseFilters(Dictionary<string, StringValues> filtersInformation)
		{
			List<IFilter<Product>> filtersList = new();
			foreach (var filterInformation in filtersInformation)
			{
				try
				{
					if (
						_factories.TryGetValue(filterInformation.Key,
						out Func<StringValues, IFilter<Product>>? factoryMethod)
					)
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
