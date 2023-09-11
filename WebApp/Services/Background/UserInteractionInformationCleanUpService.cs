using Microsoft.Extensions.Options;
using WebApp.ProjectConfiguration.Options;
using WebApp.Services.Database.Maintenance;

namespace WebApp.Services.Background
{
	public class UserInteractionInformationCleanUpService : BackgroundService
	{
		private readonly ILogger<UserInteractionInformationCleanUpService> _logger;
		private readonly IServiceProvider _services;
		private readonly int _cleanUpPeriodHours;
		private int _counter;

		public UserInteractionInformationCleanUpService(
			ILogger<UserInteractionInformationCleanUpService> logger,
			IOptions<UserInteractionOptions> options,
			IServiceProvider services)
		{
			_logger = logger;
			_cleanUpPeriodHours = options.Value.PerformCleanUpPeriodHours;
			_services = services;
			_counter = 0;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation(
				"Starting up. This service cleans up viewed and rated products information " +
				"after some period of time in order to save disk space. Do not disable this service if " +
				"you have ViewedProductsInformation and RatedProductsInformation tables in your database. " +
				"Clean up period is {cleanUpPeriod} hours.",
				_cleanUpPeriodHours
			);

			using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(_cleanUpPeriodHours));
			try
			{
				while (await timer.WaitForNextTickAsync(stoppingToken))
				{
					using (var scope = _services.CreateScope())
					{
						int counter = Interlocked.Increment(ref _counter);
						_logger.LogInformation(
							"Performing clean up. (Cleans up performed so far: {counter})",
							counter
						);

						var interactions =
							scope.ServiceProvider.GetRequiredService<UserInteractionManager>();

						await interactions.CleanUpOutdatedDataAsync(stoppingToken);
					}
				}
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("User interactions clean up service is stopping.");
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "User interactions clean up service is stopping due to an exception.");
			}
		}
	}
}
