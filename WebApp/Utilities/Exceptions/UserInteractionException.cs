namespace WebApp.Utilities.Exceptions
{
	/// <summary>
	/// Class used by controllers to notify the user about errors. This exception is NOT logged.
	/// Use other exceptions if you don't want the user to see the exception message.
	/// </summary>
	[Serializable]
	public class UserInteractionException : Exception
	{
		public UserInteractionException() { }

		public UserInteractionException(string message)
			: base(message) { }

		public UserInteractionException(string message, Exception inner)
			: base(message, inner) { }
	}
}
