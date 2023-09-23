using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account
{
	public class ChangePasswordVM
	{
		[Required(ErrorMessage = "Будь ласка, вкажіть ваш старий пароль")]
		[DataType(DataType.Password)]
		[MinLength(20, ErrorMessage = "Мінімальна довжина: 20 символів")]
		[MaxLength(1000, ErrorMessage = "Максимальна довжина: 1000 символів")]
		public string OldPassword { get; set; } = default!;

		[Required(ErrorMessage = "Будь ласка, вкажіть новий пароль")]
		[DataType(DataType.Password)]
		[MinLength(20, ErrorMessage = "Мінімальна довжина: 20 символів")]
		[MaxLength(1000, ErrorMessage = "Максимальна довжина: 1000 символів")]
		public string NewPassword { get; set; } = default!;

		[Required(ErrorMessage = "Будь ласка, повторіть новий пароль")]
		[DataType(DataType.Password)]
		[MinLength(20, ErrorMessage = "Мінімальна довжина: 20 символів")]
		[MaxLength(1000, ErrorMessage = "Максимальна довжина: 1000 символів")]
		[Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
		public string NewPasswordRepeat { get; set; } = default!;
	}
}
