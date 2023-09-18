namespace WebApp.ViewModels.Auth
{
	public class EmailConfirmationVM
	{
		public string SiteBaseAddress { get; set; } = default!;
		public int UserId { get; set; }

		public string Token { get; set; } = default!;
		public string? ReturnUrl { get; set; } = default!;
	}
}
