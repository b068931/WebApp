using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account
{
	public class ChangeEmailVM
	{
		[Required]
		[DataType(DataType.EmailAddress)]
		public string NewEmail { get; set; } = default!;
	}
}
