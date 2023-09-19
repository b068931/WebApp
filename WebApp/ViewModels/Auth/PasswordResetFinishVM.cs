using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Auth
{
	public class PasswordResetFinishVM
	{
		[Required(ErrorMessage = "Будь ласка, вкажіть ваш пароль")]
		[DataType(DataType.Password)]
		[MinLength(20, ErrorMessage = "Мінімальна довжина: 20 символів")]
		[MaxLength(1000, ErrorMessage = "Максимальна довжина: 1000 символів")]
		public string Password { get; set; } = default!;

		[Required(ErrorMessage = "Будь ласка, повторіть ваш пароль")]
		[DataType(DataType.Password)]
		[MinLength(20, ErrorMessage = "Мінімальна довжина: 20 символів")]
		[MaxLength(1000, ErrorMessage = "Максимальна довжина: 1000 символів")]
		[Compare("Password", ErrorMessage = "Паролі не співпадають")]
		public string PasswordRepeat { get; set; } = default!;

		[Required]
		public int UserId { get; set; }

		[Required]
		public string Token { get; set; } = default!;

		public string? ReturnUrl { get; set; } = default!;
	}
}
