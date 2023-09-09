using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApp.Controllers.Abstract;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Product;

namespace WebApp.Controllers.Products
{
	[Route("/product/stocks")]
	[Authorize(Roles = "admin,user")]
	public class ProductStocksController : ExtendedController
	{
		private readonly ColoursManager _colours;
		private readonly SizesManager _sizes;
		private readonly ProductStocksManager _productStocks;
		private readonly Performer<ProductStocksController> _performer;
		private readonly JsonSerializerSettings _jsonSettings;

		private ProductStocksChange GetViewModel(int productId, string? error = null)
		{
			return new ProductStocksChange()
			{
				ProductId = productId,
				ErrorMessage = error,
				Stocks = _productStocks.GetProductStocks(productId),
				Colours = _colours.GetAllColours(),
				Sizes = _sizes.GetAllSizes()
			};
		}
		private IActionResult PerformAction(Action action, int productId)
		{
			return _performer.PerformActionMessage(
				() =>
				{
					action();
					return View("Index", GetViewModel(productId));
				},
				(message) => View("Index", GetViewModel(productId, message))
			);
		}

		public ProductStocksController(
			ColoursManager colours, 
			SizesManager sizes, 
			ProductStocksManager productStocks, 
			ILogger<ProductStocksController> logger)
		{
			_colours = colours;
			_sizes = sizes;
			_productStocks = productStocks;
			_performer = new Performer<ProductStocksController>(logger);
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
			return PerformAction(
				() => _productStocks.CreateProductStocks(GetUserId(), productId, colourId, sizeId, stockSize),
				productId
			);
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
			return PerformAction(
				() => _productStocks.UpdateProductStocks(GetUserId(), stockId, colourId, sizeId, stockSize),
				productId
			);
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "productId")] int productId,
			[FromForm(Name = "stockId")] int stockId)
		{
			return PerformAction(
				() => _productStocks.DeleteProductStocks(GetUserId(), stockId),
				productId
			);
		}

		[HttpGet("json")]
		[AllowAnonymous]
		public IActionResult GetProductStocksJson(
			[FromQuery(Name = "productId")] int productId)
		{
			return Content(
						JsonConvert.SerializeObject(_productStocks.GetProductStocks(productId), _jsonSettings),
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
				Stocks = _productStocks.GetProductStocks(productId),
				Colours = _colours.GetAllColours(),
				Sizes = _sizes.GetAllSizes()
			});
		}
	}
}
