using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Products
{
	public class ProductStocksManager
	{
		private readonly DatabaseContext _database;
		private WebApp.Database.Entities.Products.ProductStock FindOwnedProductStock(
			int assumedOwner,
			int stockId)
		{
			var result = _database.ProductStocks
				.Include(e => e.Product)
				.Select(e => new
				{
					OwnerId = e.Product.ProductOwnerId,
					FoundStock = e
				})
				.FirstOrDefault(e => e.FoundStock.Id == stockId) ?? throw new UserInteractionException(
					string.Format("Product stock with id {0} does not exist", stockId)
				);

			if (result.OwnerId != assumedOwner)
				throw new UserInteractionException("You are not the owner of the product.");

			return result.FoundStock;
		}

		private bool BelongsTo(int productId, int assumedOwnerId)
		{
			return _database.Products
				.Where(e => e.Id == productId && e.ProductOwnerId == assumedOwnerId)
				.Any();
		}
		private bool StockAlreadyExists(int productId, int colourId, int sizeId)
		{
			return _database.ProductStocks
				.Where(e => e.ProductId == productId && e.ColourId == colourId && e.SizeId == sizeId)
				.Any();
		}

		public ProductStocksManager(DatabaseContext database)
		{
			_database = database;
		}

		public List<WebApp.Database.Models.ProductStock> GetProductStocks(int productId)
		{
			return _database.ProductStocks
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
				.ToList();
		}
		public void CreateProductStocks(int actionPerformerId, int productId, int colourId, int sizeId, int stockSize)
		{
			if (!BelongsTo(productId, actionPerformerId))
				throw new UserInteractionException("You are not the owner of the product.");

			if (StockAlreadyExists(productId, colourId, sizeId))
				throw new UserInteractionException("Така інформація у наявності цього продукта вже існує.");

			WebApp.Database.Entities.Products.ProductStock newStock = new WebApp.Database.Entities.Products.ProductStock()
			{
				ProductAmount = stockSize,
				ProductId = productId,
				ColourId = colourId,
				SizeId = sizeId
			};

			_database.ProductStocks.Add(newStock);
			_database.SaveChanges();
		}
		public void UpdateProductStocks(int actionPerformerId, int stockId, int colourId, int sizeId, int stockSize)
		{
			WebApp.Database.Entities.Products.ProductStock foundStock =
				FindOwnedProductStock(actionPerformerId, stockId);

			foundStock.ColourId = colourId;
			foundStock.SizeId = sizeId;
			foundStock.ProductAmount = stockSize;

			_database.SaveChanges();
		}
		public void DeleteProductStocks(int actionPerformerId, int stockId)
		{
			_database.ProductStocks.Remove(
				FindOwnedProductStock(actionPerformerId, stockId)
			);

			_database.SaveChanges();
		}
	}
}
