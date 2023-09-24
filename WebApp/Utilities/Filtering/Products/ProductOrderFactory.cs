using Microsoft.Extensions.Primitives;
using WebApp.Database.Entities.Products;
using WebApp.Utilities.Filtering.Products.OrderTypes;

namespace WebApp.Utilities.Filtering.Products
{
	public class ProductOrderFactory
	{
		private static bool IsReversed(Dictionary<string, StringValues> parameters)
		{
			bool isReversed = false;
			if (parameters.TryGetValue("ordertype", out StringValues orderType))
			{
				isReversed = orderType == "reversed";
			}

			return isReversed;
		}

		private readonly Dictionary<string, Func<int, StringValues, bool, IOrdering<Product>>> _factories;
		public ProductOrderFactory(Dictionary<string, Func<int, StringValues, bool, IOrdering<Product>>> factories)
			=> _factories = factories;

		public IOrdering<Product> CreateOrdering(
			int maxId,
			Dictionary<string, StringValues> parameters)
		{
			foreach (var factory in _factories)
			{
				if (parameters.TryGetValue(factory.Key, out StringValues value))
				{
					return factory.Value.Invoke(maxId, value, IsReversed(parameters));
				}
			}

			return IdOrder.CreateInstance(maxId);
		}
	}
}
