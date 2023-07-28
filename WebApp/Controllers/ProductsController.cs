using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
	[Route("/products")]
	public class ProductsController : Controller
	{
		private readonly DatabaseContext _database;
		private readonly ILogger<ProductsController> _logger;

		private List<SelectListItem> GetCategories()
		{
			return _database.Categories
				.AsNoTracking()
				.Where(e => e.IsLast)
				.Select(e => new SelectListItem() { Value = e.Id.ToString(), Text = e.Name })
				.ToList();
		}
		private List<SelectListItem> GetBrands()
		{
			return _database.Brands
				.AsNoTracking()
				.Select(e => new SelectListItem() { Value = e.Id.ToString(), Text = e.Name })
				.ToList();
		}
		private ProductCreation GetProductCreationVM()
		{
			return new ProductCreation()
			{
				AvailableCategories = GetCategories(),
				AvailableBrands = GetBrands()
			};
		}

		private IActionResult PerformAction(Action action)
		{
			try
			{
				action();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "ProductsController error.");
				ViewBag.InternalError = "Something went really wrong.";
			}

			return View("ProductCreationForm", GetProductCreationVM());
		}

		public ProductsController(DatabaseContext database, ILogger<ProductsController> logger)
		{
			_database = database;
			_logger = logger;
		}

		[HttpGet("action/create")]
		public IActionResult Create()
		{
			ViewBag.InternalError = "";
			return View("ProductCreationForm", GetProductCreationVM());
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(ProductCreation vm)
		{
			if(!ModelState.IsValid)
			{
				return View("ProductCreationForm", GetProductCreationVM());
			}

			return PerformAction();
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
