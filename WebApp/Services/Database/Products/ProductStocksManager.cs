using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Products
{
	public class ProductStocksManager
	{
		private readonly DatabaseContext _database;

		private async Task<bool> StockAlreadyExistsAsync(int productId, int colourId, int sizeId)
		{
			return await _database.ProductStocks
				.Where(e => e.ProductId == productId && e.ColourId == colourId && e.SizeId == sizeId)
				.AnyAsync();
		}
		private async Task<ProductStock> FindProductStockAsync(int stockId)
		{
			ProductStock result = await _database.ProductStocks
				.FindAsync(stockId) ?? throw new UserInteractionException(
					string.Format("Product stock with id {0} does not exist", stockId)
				);

			return result;
		}

		public ProductStocksManager(DatabaseContext database)
		{
			_database = database;
		}

		public async Task<List<WebApp.Database.Models.ProductStock>> GetProductStocksAsync(int productId)
		{
			return await _database.ProductStocks
				.AsNoTracking()
				.Include(e => e.Colour)
				.Include(e => e.Size)
				.Where(e => e.ProductId == productId)
				.Select(e => new WebApp.Database.Models.ProductStock()
				{
					Id = e.Id,
					ProductAmount = e.ProductAmount,
					Colour = new WebApp.Database.Models.Colour()
					{
						Id = e.Colour.Id,
						Name = e.Colour.Name,
						HexCode = e.Colour.HexCode
					},
					Size = new WebApp.Database.Models.Size()
					{
						Id = e.Size.Id,
						Name = e.Size.SizeName
					}
				})
				.ToListAsync();
		}
		public async Task CreateProductStocksAsync(int productId, int colourId, int sizeId, int stockSize)
		{
			if (await StockAlreadyExistsAsync(productId, colourId, sizeId))
				throw new UserInteractionException("Така інформація у наявності цього продукта вже існує.");

			ProductStock newStock = new ProductStock()
			{
				ProductAmount = stockSize,
				ProductId = productId,
				ColourId = colourId,
				SizeId = sizeId
			};

			_database.ProductStocks.Add(newStock);
			await _database.SaveChangesAsync();
		}
		public async Task UpdateProductStocksAsync(int stockId, int colourId, int sizeId, int stockSize)
		{
			ProductStock foundStock = await FindProductStockAsync(stockId);

			foundStock.ColourId = colourId;
			foundStock.SizeId = sizeId;
			foundStock.ProductAmount = stockSize;

			await _database.SaveChangesAsync();
		}
		public async Task DeleteProductStocksAsync(int stockId)
		{
			_database.ProductStocks.Remove(
				await FindProductStockAsync(stockId)
			);

			await _database.SaveChangesAsync();
		}
	}
}
