namespace WebApp.Helpers
{
	[Serializable]
	public class Page
	{
		public static int DefaultPageSize = 1;

		public int PageStartId { get; set; }
		public int HighestId { get; set; }

		public int PageSize { get; set; } = Page.DefaultPageSize;
	}
}
