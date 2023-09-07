namespace WebApp.Utilities.Filtering
{
	public interface IFilter<T> where T : class
	{
		IQueryable<T> Apply(IQueryable<T> request);
	}
}
