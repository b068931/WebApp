using Microsoft.AspNetCore.Mvc;
using WebApp.Utilities.Exceptions;

namespace WebApp.Utilities.Other
{
	public class Performer<T> where T : Controller
	{
		private const string UncaughtExceptionMessageTemplate = "{ControllerName} error. See the exception message for details.";
		private const string DefaultPerformActionMessage = "[SERVER INNER ERROR]";

		private readonly ILogger<T> _logger;
		public Performer(ILogger<T> logger)
		{
			_logger = logger;
		}

		public IActionResult PerformAction(
			Func<IActionResult> action,
			Action<UserInteractionException>? onUserError,
			Func<IActionResult> onFail)
		{
			try
			{
				return action();
			}
			catch (UserInteractionException ex)
			{
				onUserError?.Invoke(ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex, 
					UncaughtExceptionMessageTemplate, 
					typeof(T).FullName
				);
			}

			return onFail();
		}

		public IActionResult PerformActionMessage(
			Func<IActionResult> action,
			Func<string, IActionResult> onFail)
		{
			string savedMesssage = DefaultPerformActionMessage;
			return PerformAction(
				action,
				(ex) => savedMesssage = ex.Message,
				() => onFail(savedMesssage)
			);
		}

		public IActionResult PerformInertAction(
			Func<IActionResult> action,
			Func<IActionResult> onFail)
		{
			return PerformAction(action, null, onFail);
		}

		public async Task<IActionResult> PerformActionAsync(
			Func<Task<IActionResult>> action,
			Action<UserInteractionException>? onUserError,
			Func<Task<IActionResult>> onFail)
		{
			try
			{
				return await action();
			}
			catch (UserInteractionException ex)
			{
				onUserError?.Invoke(ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					UncaughtExceptionMessageTemplate,
					typeof(T).FullName
				);
			}

			return await onFail();
		}

		public async Task<IActionResult> PerformActionMessageAsync(
			Func<Task<IActionResult>> action,
			Func<string, Task<IActionResult>> onFail)
		{
			string savedMessage = DefaultPerformActionMessage;
			return await PerformActionAsync(
				action,
				(ex) => savedMessage = ex.Message,
				() => onFail(savedMessage)
			);
		}

		public async Task<IActionResult> PerformInertAction(
			Func<Task<IActionResult>> action,
			Func<Task<IActionResult>> onFail)
		{
			return await PerformActionAsync(action, null, onFail);
		}
	}
}
