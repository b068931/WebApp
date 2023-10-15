using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebApp.Database.Entities.Auth;
using WebApp.ProjectConfiguration.Options;

namespace WebApp.Utilities.CustomIdentityComponents
{
	public class AccountDeletionEmailConfirmationTokenProvider : DataProtectorTokenProvider<ApplicationUser>
	{
		public AccountDeletionEmailConfirmationTokenProvider(
			IDataProtectionProvider dataProtectionProvider,
			IOptions<AccountDeletionEmailConfirmationTokenProviderOptions> options,
			ILogger<DataProtectorTokenProvider<ApplicationUser>> logger)
			: base(dataProtectionProvider, options, logger)
		{
		}
	}
}
