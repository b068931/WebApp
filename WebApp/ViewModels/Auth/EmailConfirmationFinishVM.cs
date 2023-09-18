namespace WebApp.ViewModels.Auth
{
	public class EmailConfirmationFinishVM
	{
		public string? ReturnUrl { get; set; } = default!;
		public string? Error { get; set; } = default!;
	}
}
