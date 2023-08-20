namespace WebApp.Helpers
{
	public interface IOrdering<T> where T : class
	{
		IQueryable<T> Apply(IQueryable<T> request);
	}
}
