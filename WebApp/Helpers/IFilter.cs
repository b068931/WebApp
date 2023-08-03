using System.Runtime.Serialization;

namespace WebApp.Helpers
{
	public interface IFilter<T> where T : class
	{
		IEnumerable<T> Apply(IEnumerable<T> request);
	}
}
