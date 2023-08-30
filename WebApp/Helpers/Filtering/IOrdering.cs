namespace WebApp.Helpers.Filtering
{
    public interface IOrdering<T> where T : class
    {
        IQueryable<T> Apply(IQueryable<T> request);
    }
}
