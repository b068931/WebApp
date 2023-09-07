using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApp.Database.Entities.Auth
{
	public class ApplicationRoleStore : RoleStore<ApplicationRole, DatabaseContext, int, ApplicationUserRole, ApplicationRoleClaim>
	{
		public ApplicationRoleStore(DatabaseContext context)
			: base(context)
		{
		}
	}
}
