using Microsoft.AspNetCore.Identity;
using WebApp.Database.Entities.Auth;
using WebApp.ProjectConfiguration.Options.Auth;
using WebApp.Utilities.Exceptions;

namespace WebApp.ProjectConfiguration.Initializers
{
	public class DatabaseInitializer
	{
		private readonly UserManager<ApplicationUser> _users;
		private readonly RoleManager<ApplicationRole> _roles;
		private readonly IConfiguration _configuration;

		public DatabaseInitializer(
			UserManager<ApplicationUser> users,
			RoleManager<ApplicationRole> roles,
			IConfiguration configuration)
		{
			_users = users;
			_roles = roles;
			_configuration = configuration;
		}

		private async Task PopulateUserAsync(string userHandle, string userName, string userRole)
		{
			string emailProperty = "Preadded" + userHandle + "Email";
			string passwordProperty = "Preadded" + userHandle + "Password";

			string email = _configuration[emailProperty] ??
				throw new ArgumentNullException(
					$"Unable to find property '{emailProperty}' it is required in order to create a/an '{userHandle}' account. (check user secrets)"
				);

			string password = _configuration[passwordProperty] ??
				throw new ArgumentNullException(
					$"Unable to find property '{passwordProperty}' it is required in order to create a/an '{userHandle}' account. (check user secrets)"
				);

			if (await _users.FindByEmailAsync(email) == null)
			{
				var newUser = new ApplicationUser()
				{
					UserName = userName,
					Email = email,
					EmailConfirmed = true
				};

				IdentityResult result = await _users.CreateAsync(newUser, password);
				if (result.Succeeded)
				{
					await _users.AddToRoleAsync(newUser, userRole);
				}
				else
				{
					throw new ServerInitializationException(
						string.Join(", ", result.Errors.Select(e => e.Description))
					);
				}
			}
		}
		private async Task InitializeRoles(IEnumerable<string> roles)
		{
			foreach (var role in roles)
			{
				if (await _roles.FindByNameAsync(role) == null)
				{
					await _roles.CreateAsync(new ApplicationRole(role));
				}
			}
		}
		private async Task InitializeUsers(IEnumerable<ConfiguredUser> users)
		{
			foreach (var user in users)
			{
				await PopulateUserAsync(user.SecretsHandle, user.Name, user.Role);
			}
		}

		public async Task InitializeAuthAsync(AuthConfiguration preaddedInformation)
		{
			await InitializeRoles(preaddedInformation.PreaddedRoles);
			await InitializeUsers(preaddedInformation.PreaddedUsers);
		}
	}
}
