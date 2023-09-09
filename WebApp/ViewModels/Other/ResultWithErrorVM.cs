namespace WebApp.ViewModels.Other
{
	public class ResultWithErrorVM<T> where T : class
	{
		public T Result { get; set; } = default!;
		public string Error { get; set; } = default!;
	}
}
