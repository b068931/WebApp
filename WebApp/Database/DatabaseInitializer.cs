using Microsoft.AspNetCore.Identity;
using WebApp.Database.Entities.Auth;
using WebApp.Helpers.Exceptions;

namespace WebApp.Database
{
	public class DatabaseInitializer
	{
		private static string[] _baseRoles = { "admin", "user" };
		private static (string handle, string name, string role)[] _baseUsers = 
			{ 
				("Admin", "Site Administration", "admin"), 
				("TestUser", "Dummy", "user")
			};

		private readonly UserManager<ApplicationUser> _users;
		private readonly RoleManager<ApplicationRole> _roles;
		private readonly IConfiguration _configuration;

		public DatabaseInitializer(UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles, IConfiguration configuration)
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
					string.Format(
						"Unable to find property '{0}' it is required in order to create a/an '{1}' account. (check user secrets)",
						emailProperty,
						userHandle
					)
				);

			string password = _configuration[passwordProperty] ??
				throw new ArgumentNullException(
					string.Format(
						"Unable to find property '{0}' it is required in order to create a/an '{1}' account. (check user secrets)",
						passwordProperty,
						userHandle
					)
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
		private async Task InitializeRoles(string[] roles)
		{
			foreach (var role in roles)
			{
				if (await _roles.FindByNameAsync(role) == null)
				{
					await _roles.CreateAsync(new ApplicationRole(role));
				}
			}
		}
		private async Task InitializeUsers((string handle, string name, string role)[] users)
		{
			foreach (var user in users)
			{
				await PopulateUserAsync(user.handle, user.name, user.role);
			}
		}

		public async Task InitializeAuthAsync()
		{
			await InitializeRoles(_baseRoles);
			await InitializeUsers(_baseUsers);
		}
	}
}
