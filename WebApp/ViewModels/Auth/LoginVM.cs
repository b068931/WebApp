using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Auth
{
	public class LoginVM
	{
		[Required(ErrorMessage = "Будь ласка, вкажіть ваш логін")]
		[DataType(DataType.Text)]
		[MinLength(2, ErrorMessage = "Мінімальна довжина: 2 символи")]
		[MaxLength(25, ErrorMessage = "Максимальна довжина: 25 символів")]
		public string UserName { get; set; } = default!;

		[Required(ErrorMessage = "Будь ласка, вкажіть ваш пароль")]
		[DataType(DataType.Password)]
		[MinLength(20, ErrorMessage = "Мінімальна довжина: 20 символів")]
		[MaxLength(1000, ErrorMessage = "Максимальна довжина: 1000 символів")]
		public string Password { get; set; } = default!;

		[MaxLength(1000)]
		public string? ReturnUrl { get; set; } = default!;
	}
}
