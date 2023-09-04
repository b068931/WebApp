using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Auth
{
	public class LoginVM
	{
		[Required(ErrorMessage = "Будь ласка, вкажіть вашу поштову адресу")]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; } = default!;

		[Required(ErrorMessage = "Будь ласка, вкажіть ваш пароль")]
		[DataType(DataType.Password)]
		[MinLength(10, ErrorMessage = "Мінімальна довжина: 10 символів")]
		[MaxLength(1000, ErrorMessage = "Максимальна довжина: 1000 символів")]
		public string Password { get; set; } = default!;

		[MaxLength(1000)]
		public string? ReturnUrl { get; set; } = default!;
	}
}
