namespace WebApp.Utilities.Exceptions
{
	[Serializable]
	public class ServerInitializationException : Exception
	{
		public ServerInitializationException() { }

		public ServerInitializationException(string message)
			: base(message) { }

		public ServerInitializationException(string message, Exception inner)
			: base(message, inner) { }
	}
}
