using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApp.Utilities.CustomRequirements.SameAuthor;
using WebApp.Utilities.Exceptions;

namespace WebApp.Utilities.Other
{
	public class Performer<T> where T : Controller
	{
		private const string UncaughtExceptionMessageTemplate = "See the exception message for details.";
		private const string DatabaseExceptionMessage = "Unable to execute the database query. See the exception from EntityFramework.";
		private const string OnFailFail = "Something went so wrong that even the 'onFail' method has failed. See the exception for details.";
		private const string DefaultPerformActionMessage = "[SERVER_INNER_ERROR]";

		private readonly ILogger<T> _logger;
		private readonly IAuthorizationService _authorization;

		private async Task<IActionResult> InnerPerformActionAsync(
			Func<Task<IActionResult>> action,
			Action<UserInteractionException>? onUserError,
			Func<object> onFail)
		{
			try
			{
				return await action();
			}
			catch (UserInteractionException ex)
			{
				onUserError?.Invoke(ex);
			}
			catch (DbUpdateException)
			{
				onUserError?.Invoke(new UserInteractionException("Помилка виконання запиту до бази даних. Перезавантажте сторінку."));
				_logger.LogWarning(DatabaseExceptionMessage);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					UncaughtExceptionMessageTemplate,
					typeof(T).FullName
				);
			}

			try
			{
				object failResult = onFail();
				if (failResult is Task<IActionResult> asynchronousResult)
				{
					return await asynchronousResult;
				}
				else
				{
					return (IActionResult)failResult;
				}
			}
			catch (Exception exc)
			{
				_logger.LogError(exc, OnFailFail);
			}

			return new StatusCodeResult(StatusCodes.Status500InternalServerError);
		}

		public Performer(
			ILogger<T> logger,
			IAuthorizationService authorization)
		{
			_logger = logger;
			_authorization = authorization;
		}

		public Task<IActionResult> PerformActionAsync(
			Func<Task<IActionResult>> action,
			Action<UserInteractionException>? onUserError,
			Func<Task<IActionResult>> onFail)
		{
			return InnerPerformActionAsync(action, onUserError, onFail);
		}

		public Task<IActionResult> PerformActionAsync(
			Func<Task<IActionResult>> action,
			Action<UserInteractionException>? onUserError,
			Func<IActionResult> onFail)
		{
			return InnerPerformActionAsync(action, onUserError, onFail);
		}

		public Task<IActionResult> PerformActionMessageAsync(
			Func<Task<IActionResult>> action,
			Func<string, Task<IActionResult>> onFail)
		{
			string savedMessage = DefaultPerformActionMessage;
			return PerformActionAsync(
				action,
				(ex) => savedMessage = ex.Message,
				() => onFail(savedMessage)
			);
		}

		public Task<IActionResult> PerformActionMessageAsync(
			Func<Task<IActionResult>> action,
			Func<string, IActionResult> onFail)
		{
			string savedMessage = DefaultPerformActionMessage;
			return PerformActionAsync(
				action,
				(ex) => savedMessage = ex.Message,
				() => onFail(savedMessage)
			);
		}

		public Task<IActionResult> PerformInertActionAsync(
			Func<Task<IActionResult>> action,
			Func<Task<IActionResult>> onFail)
		{
			return PerformActionAsync(action, null, onFail);
		}

		public Task<IActionResult> PerformInertActionAsync(
			Func<Task<IActionResult>> action,
			Func<IActionResult> onFail)
		{
			return PerformActionAsync(action, null, onFail);
		}

		public async Task<IActionResult> EnforceSameAuthorWrapperAsync(
			Func<Task<Author>> authorProvider,
			ClaimsPrincipal currentUser,
			Func<Task<IActionResult>> action)
		{
			var authorizationResult =
				await _authorization.AuthorizeAsync(currentUser, await authorProvider(), "MyContentPolicy");

			if (authorizationResult.Succeeded)
			{
				return await action();
			}
			else if (currentUser.Identity?.IsAuthenticated ?? false)
			{
				return new ForbidResult();
			}
			else
			{
				return new ChallengeResult();
			}
		}
	}
}
