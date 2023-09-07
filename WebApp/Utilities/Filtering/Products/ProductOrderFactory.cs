using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;
using WebApp.Utilities.Filtering.Products.OrderTypes;

namespace WebApp.Utilities.Filtering.Products
{
	public class ProductOrderFactory
	{
		private static bool isReversed(Dictionary<string, StringValues> parameters)
		{
			bool isReversed = false;
			StringValues orderType;
			if (parameters.TryGetValue("ordertype", out orderType))
			{
				isReversed = orderType == "reversed";
			}

			return isReversed;
		}

		private Dictionary<string, Func<int, StringValues, bool, IOrdering<Product>>> _factories;
		public ProductOrderFactory(Dictionary<string, Func<int, StringValues, bool, IOrdering<Product>>> factories)
			=> _factories = factories;

		public IOrdering<Product> CreateOrdering(
			int maxId,
			Dictionary<string, StringValues> parameters)
		{
			foreach (var factory in _factories)
			{
				StringValues value;
				if (parameters.TryGetValue(factory.Key, out value))
				{
					return factory.Value.Invoke(maxId, value, isReversed(parameters));
				}
			}

			return IdOrder.CreateInstance(maxId);
		}
	}
}
