using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
				.Where(e => e.IsLast)
				.Select(e => new SelectListItem() { Value = e.Id.ToString(), Text = e.Name })
				.ToList();
		}
		private List<SelectListItem> GetBrands()
		{
			return _database.Brands
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

		public ProductsController(DatabaseContext database, ILogger<ProductsController> logger)
		{
			_database = database;
			_logger = logger;
		}

		[HttpGet("action/create")]
		public IActionResult Create()
		{
			return View("ProductCreationForm", GetProductCreationVM());
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
