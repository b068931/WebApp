using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApp.Database.Entities.Auth
{
	public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, DatabaseContext, int, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>
	{
		public ApplicationUserStore(DatabaseContext context) 
			: base(context)
		{
		}
	}
}
