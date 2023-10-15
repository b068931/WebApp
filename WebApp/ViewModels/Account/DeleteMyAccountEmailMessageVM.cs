namespace WebApp.ViewModels.Account
{
	public class DeleteMyAccountEmailMessageVM
	{
		public string SiteBaseAddress { get; set; } = default!;

		public int UserId { get; set; }
		public string Token { get; set; } = default!;
		public string UserName { get; set; } = default!;
	}
}
