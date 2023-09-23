using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account
{
	public class ChangeUserNameVM
	{
		[Required(ErrorMessage = "Будь ласка, вкажіть ваш новий логін")]
		[MinLength(2, ErrorMessage = "Мінімальна довжина: 2 символи")]
		[MaxLength(25, ErrorMessage = "Максимальна довжина: 25 символів")]
		public string UserName { get; set; } = default!;
	}
}
