using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Auth
{
	public class ForgotPasswordVM
	{
		[Required]
		public string Email { get; set; } = default!;
		public string? ReturnUrl { get; set; }
	}
}
