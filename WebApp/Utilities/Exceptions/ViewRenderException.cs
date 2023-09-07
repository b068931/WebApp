namespace WebApp.Utilities.Exceptions
{
	[Serializable]
	public class ViewRenderException : Exception
	{
		public ViewRenderException() { }

		public ViewRenderException(string message)
			: base(message) { }

		public ViewRenderException(string message, Exception inner)
			: base(message, inner) { }
	}
}
