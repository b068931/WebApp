using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApp.Utilities.CustomRequirements.SameAuthor
{
	public class SameAuthorAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Author>
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			SameAuthorRequirement requirement,
			Author trueAuthor)
		{
			string? userIdentifier = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userIdentifier != null)
			{
				if (int.Parse(userIdentifier) == trueAuthor.Id)
				{
					context.Succeed(requirement);
				}
			}

			return Task.CompletedTask;
		}
	}
}
