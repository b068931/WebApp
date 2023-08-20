using WebApp.Database.Entities;
using WebApp.Helpers.Products.Filtering.OrderTypes;

namespace WebApp.Helpers.Products.Filtering
{
	public class ProductOrderFactory
	{
		private static bool isReversed(Dictionary<string, string> parameters)
		{
			bool isReversed = false;
			string? orderType;
			if (parameters.TryGetValue("ordertype", out orderType))
			{
				isReversed = orderType == "reversed";
			}

			return isReversed;
		}

		private Dictionary<string, Func<int, string, bool, IOrdering<Product>>> _factories;
		public ProductOrderFactory(Dictionary<string, Func<int, string, bool, IOrdering<Product>>> factories)
			=> _factories = factories;

		public IOrdering<Product> CreateOrdering(
			int maxId, 
			Dictionary<string, string> parameters)
		{
			foreach(var factory in _factories)
			{
				string? value;
				if (parameters.TryGetValue(factory.Key, out value))
				{
					return factory.Value.Invoke(maxId, value, isReversed(parameters));
				}
			}

			return IdOrder.CreateInstance(maxId);
		}
	}
}
