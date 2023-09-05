using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Database.Products;
using WebApp.ViewModels.Product;

namespace WebApp.Controllers.Products
{
    [Route("/product/stocks")]
	public class ProductStocksController : Controller
	{
		private readonly ColoursManager _colours;
		private readonly SizesManager _sizes;
		private readonly ProductsManager _products;
		private readonly ILogger<ProductStocksController> _logger;
		private readonly JsonSerializerSettings _jsonSettings;

		private IActionResult PerformAction(Action action, int productId)
		{
			string? error = null;
			try
			{
				action();
			}
			catch(UserInteractionException exc)
			{
				error = exc.Message;
			}
			catch(DbUpdateException)
			{
				error = "Ви не можете створювати продукти у наявності з однаковим кольором та розміром. " +
					"Якщо ви вважаєте, що ця помилка не є правильною, то перезавантажте сторінку та спробуйте ще раз.";
			}
			catch(Exception exc)
			{
				error = "Unexpected error.";
				_logger.LogError(exc, "ProductStocksController error. See exception for details.");
			}

			return View("Index", new ProductStocksChange()
			{
				ProductId = productId,
				ErrorMessage = error,
				Stocks = _products.GetProductStocks(productId),
				Colours = _colours.GetAllColours(),
				Sizes = _sizes.GetAllSizes()
			});
		}

		public ProductStocksController(ColoursManager colours, SizesManager sizes, ProductsManager products, ILogger<ProductStocksController> logger)
		{
			_colours = colours;
			_sizes = sizes;
			_products = products;
			_logger = logger;
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
		public IActionResult Create(
			[FromForm(Name = "productId")] int productId,
			[FromForm(Name = "stockColour")] int colourId,
			[FromForm(Name = "stockProductsSize")] int sizeId,
			[FromForm(Name = "stockSize")] int stockSize)
		{
			return PerformAction(() => _products.CreateProductStocks(productId, colourId, sizeId, stockSize), productId);
		}

		[HttpPost("action/update/{stockId}")]
		[ValidateAntiForgeryToken]
		public IActionResult Update(
			[FromForm(Name = "productId")] int productId,
			[FromRoute(Name = "stockId")] int stockId,
			[FromForm(Name = "stockColour")] int colourId,
			[FromForm(Name = "stockProductsSize")] int sizeId,
			[FromForm(Name = "stockSize")] int stockSize)
		{
			return PerformAction(() => _products.UpdateProductStocks(stockId, colourId, sizeId, stockSize), productId);
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "productId")] int productId,
			[FromForm(Name = "stockId")] int stockId)
		{
			return PerformAction(() => _products.DeleteProductStocks(stockId), productId);
		}

		[HttpGet("json")]
		public IActionResult GetProductStocksJson(
			[FromQuery(Name = "productId")] int productId)
		{
			return Content(
						JsonConvert.SerializeObject(_products.GetProductStocks(productId), _jsonSettings),
						"application/json"
					);
		}

		public IActionResult Index(
			[FromQuery(Name = "id")] int productId)
		{
			return View("Index", new ProductStocksChange()
			{
				ProductId = productId,
				ErrorMessage = null,
				Stocks = _products.GetProductStocks(productId),
				Colours = _colours.GetAllColours(),
				Sizes = _sizes.GetAllSizes()
			});
		}
	}
}
