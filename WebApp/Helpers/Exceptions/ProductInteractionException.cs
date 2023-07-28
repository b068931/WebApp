namespace WebApp.Helpers.Exceptions
{
	/// <summary>
	/// Class used by ProductsController to notify the user about errors. This exception is NOT logged.
	/// Use other exceptions if you don't want the user to see the exception message.
	/// </summary>
	[Serializable]
	public class ProductInteractionException : Exception
	{
		public ProductInteractionException() { }

		public ProductInteractionException(string message) 
			:base(message) { }

		public ProductInteractionException(string message, Exception inner)
			:base(message, inner) { }
	}
}
