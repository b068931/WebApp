using Microsoft.AspNetCore.Identity;

namespace WebApp.Database.Entities.Auth
{
	public class ApplicationRole : IdentityRole<int>
	{
		public ApplicationRole() { }
		public ApplicationRole(string roleName) { Name = roleName; }
	}
}
