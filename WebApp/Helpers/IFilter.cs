using System.Runtime.Serialization;

namespace WebApp.Helpers
{
	public interface IFilter<T> where T : class
	{
		IQueryable<T> Apply(IQueryable<T> request);
	}
}
