using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using WebApp.Database.Entities.Auth;
using WebApp.Extensions;

namespace WebApp.Utilities.CustomIdentityComponents
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> options
        ) : base(userManager, roleManager, options)
        { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            ClaimsIdentity userIdentity = await base.GenerateClaimsAsync(user);

            userIdentity.AddClaim(
                new Claim(
                    ApplicationClaimTypes.AccountCreationDate,
                    user.AccountCreationDate.ToString()
                )
            );

            userIdentity.AddClaim(
                new Claim(
                    ApplicationClaimTypes.HasPasswordAuthentication,
                    (user.PasswordHash != null).ToString()
                )
            );

            return userIdentity;
        }
    }
}
