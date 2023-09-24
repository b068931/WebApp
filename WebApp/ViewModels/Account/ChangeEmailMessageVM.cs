namespace WebApp.ViewModels.Account
{
	public class ChangeEmailMessageVM
	{
		public string SiteBaseAddress { get; set; } = default!;

		public int UserId { get; set; }
		public string Token { get; set; } = default!;
		public string NewEmail { get; set; } = default!;
	}
}
