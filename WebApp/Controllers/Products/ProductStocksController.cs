using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApp.Controllers.Abstract;
using WebApp.Database.Models.Stocks;
using WebApp.Services.Actions;
using WebApp.Services.Database.Products;
using WebApp.Utilities.CustomRequirements.SameAuthor;
using WebApp.Utilities.Exceptions;
using WebApp.ViewModels.Product;

namespace WebApp.Controllers.Products
{
    [Route("/product/stocks")]
	[Authorize(Policy = "PublicContentPolicy")]
	public class ProductStocksController : ExtendedController
	{
		private readonly ColoursManager _colours;
		private readonly SizesManager _sizes;

		private readonly ProductStocksManager _productStocks;
		private readonly ProductsManager _products;

		private readonly Performer<ProductStocksController> _performer;
		private readonly JsonSerializerSettings _jsonSettings;

		private async Task<ProductStocksChange> GetViewModelAsync(int productId, string? error = null)
		{
			return new()
			{
				ProductId = productId,
				ErrorMessage = error,
				Stocks = await _productStocks.GetProductStocksAsync(productId),
				Colours = await _colours.GetAllColoursAsync(),
				Sizes = await _sizes.GetAllSizesAsync()
			};
		}
		private Task<IActionResult> PerformActionAsync(
			Func<Task> action,
			int productId,
			int? stockId)
		{
			return _performer.PerformActionMessageAsync(
				() => _performer.EnforceSameAuthorWrapperAsync(
					async () =>
					{
						int trueAuthorId;
						if (stockId == null)
						{
							trueAuthorId = await _products.GetProductOwnerAsync(productId);
						}
						else
						{
							ProductStockOwnershipModel productStockOwnership =
								await _productStocks.GetProductStockOwnerAsync(stockId.Value);

							if (productStockOwnership.ProductId != productId)
								throw new UserInteractionException("ProductId information mismatch.");

							trueAuthorId = productStockOwnership.OwnerId;
						}

						return new Author()
						{
							Id = trueAuthorId
						};
					},
					User,
					async () =>
					{
						await action();
						return View("Index", await GetViewModelAsync(productId));
					}
				),
				async (message) => View("Index", await GetViewModelAsync(productId, message))
			);
		}

		public ProductStocksController(
			ColoursManager colours,
			SizesManager sizes,
			ProductStocksManager productStocks,
			ProductsManager products,
			Performer<ProductStocksController> performer)
		{
			_colours = colours;
			_sizes = sizes;
			_productStocks = productStocks;
			_products = products;
			_performer = performer;
			_jsonSettings = new JsonSerializerSettings()
			{
				ContractResolver = new DefaultContractResolver()
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				}
			};
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Create(
			[FromForm(Name = "productId")] int productId,
			[FromForm(Name = "stockColour")] int colourId,
			[FromForm(Name = "stockProductsSize")] int sizeId,
			[FromForm(Name = "stockSize")] int stockSize)
		{
			return PerformActionAsync(
				() => _productStocks.CreateProductStocksAsync(productId, colourId, sizeId, stockSize),
				productId,
				null
			);
		}

		[HttpPost("action/update/{stockId}")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Update(
			[FromForm(Name = "productId")] int productId,
			[FromRoute(Name = "stockId")] int stockId,
			[FromForm(Name = "stockColour")] int colourId,
			[FromForm(Name = "stockProductsSize")] int sizeId,
			[FromForm(Name = "stockSize")] int stockSize)
		{
			return PerformActionAsync(
				() => _productStocks.UpdateProductStocksAsync(stockId, colourId, sizeId, stockSize),
				productId,
				stockId
			);
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Delete(
			[FromForm(Name = "productId")] int productId,
			[FromForm(Name = "stockId")] int stockId)
		{
			return PerformActionAsync(
				() => _productStocks.DeleteProductStocksAsync(stockId),
				productId,
				stockId
			);
		}

		[HttpGet("json")]
		[AllowAnonymous]
		public async Task<IActionResult> GetProductStocksJson(
			[FromQuery(Name = "productId")] int productId)
		{
			return Content(
						JsonConvert.SerializeObject(
							await _productStocks.GetProductStocksAsync(productId),
							_jsonSettings
						),
						"application/json"
					);
		}

		public async Task<IActionResult> Index(
			[FromQuery(Name = "id")] int productId)
		{
			return View("Index", new ProductStocksChange()
			{
				ProductId = productId,
				ErrorMessage = null,
				Stocks = await _productStocks.GetProductStocksAsync(productId),
				Colours = await _colours.GetAllColoursAsync(),
				Sizes = await _sizes.GetAllSizesAsync()
			});
		}
	}
}
