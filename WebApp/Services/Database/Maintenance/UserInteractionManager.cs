using Microsoft.Extensions.Options;
using WebApp.Database;
using WebApp.Database.Entities.Abstract;
using WebApp.Database.Entities.UserInteractions;
using WebApp.ProjectConfiguration.Options;

namespace WebApp.Services.Database.Maintenance
{
	public class UserInteractionManager
	{
		private readonly DatabaseContext _database;
		private readonly UserInteractionOptions _options;

		private DateOnly GetRecentDate(int daysToSubtract)
		{
			return DateOnly.FromDateTime(DateTime.Now).AddDays(-daysToSubtract);
		}
		private async Task<bool> GenericUpdateUserProductMappingWithDateAsync<T>(int userId, int productId) where T : UserProductMappingWithDate, new()
		{
			T? lastInteraction =
				(T?)await _database.FindAsync(typeof(T), userId, productId);

			if (lastInteraction != null)
			{
				lastInteraction.InteractionDate = GetRecentDate(0);
				await _database.SaveChangesAsync();

				return false;
			}
			else
			{
				T newView = new T()
				{
					UserId = userId,
					ProductId = productId,
					InteractionDate = GetRecentDate(0)
				};

				_database.Add(newView);
				await _database.SaveChangesAsync();

				return true;
			}
		}

		private Task CleanUpProductsViewDataAsync(CancellationToken stoppingToken)
		{
			DateOnly minCleanUpDate = GetRecentDate(_options.StoreViewInformationDays);
			_database.ViewedProducts.RemoveRange(
				_database.ViewedProducts
					.Where(e => e.InteractionDate <= minCleanUpDate)
			);

			return _database.SaveChangesAsync(stoppingToken);
		}
		private Task CleanUpProductsRatedDataAsync(CancellationToken stoppingToken)
		{
			DateOnly minCleanUpDate = GetRecentDate(_options.StoreRatedInformationDays);
			_database.RatedProducts.RemoveRange(
				_database.RatedProducts
					.Where(e => e.InteractionDate <= minCleanUpDate)
			);

			return _database.SaveChangesAsync(stoppingToken);
		}

		public UserInteractionManager(
			DatabaseContext database, 
			IOptions<UserInteractionOptions> options)
		{
			_database = database;
			_options = options.Value;
		}

		public async Task CleanUpOutdatedDataAsync(CancellationToken stoppingToken)
		{
			await CleanUpProductsViewDataAsync(stoppingToken);
			await CleanUpProductsRatedDataAsync(stoppingToken);
		}

		public Task<bool> RememberViewedProductAsync(int userId, int productId)
		{
			return GenericUpdateUserProductMappingWithDateAsync<ViewedProductsInformation>(userId, productId);
		}
		public Task<bool> RememberRatedProductAsync(int userId, int productId)
		{
			return GenericUpdateUserProductMappingWithDateAsync<RatedProductsInformation>(userId, productId);
		}
	}
}
