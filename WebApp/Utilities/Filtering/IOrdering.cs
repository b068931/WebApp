namespace WebApp.Utilities.Filtering
{
	public interface IOrdering<T> where T : class
	{
		IQueryable<T> Apply(IQueryable<T> request);
	}
}
