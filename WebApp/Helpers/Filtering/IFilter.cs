using System.Runtime.Serialization;

namespace WebApp.Helpers.Filtering
{
    public interface IFilter<T> where T : class
    {
        IQueryable<T> Apply(IQueryable<T> request);
    }
}
