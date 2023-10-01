using Newtonsoft.Json;

namespace WebApp.Extensions
{
	public static class CookieCollectionExtensions
	{
		public static List<int> GetIntsList(
			this IRequestCookieCollection cookieCollection,
			string key)
		{
			List<int> ints = new List<int>();
			if(cookieCollection.TryGetValue(key, out string? serializedInts))
			{
				ints = JsonConvert.DeserializeObject<List<int>>(serializedInts ?? "") 
						?? new List<int>();
			}

			return ints;
		}

		public static void SetIntsList(
			this IResponseCookies cookieCollection,
			string key,
			List<int> values,
			CookieOptions options)
		{
			cookieCollection.Append(
				key,
				JsonConvert.SerializeObject(values),
				options
			);
		}
	}
}
