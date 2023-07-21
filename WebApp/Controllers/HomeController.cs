using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApp.Database;
using WebApp.Models;

namespace WebApp.Controllers
{
	public class HomeController : Controller
	{
		private readonly DatabaseContext _database;
		public HomeController(DatabaseContext database)
		{
			_database = database;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpGet("/categories/category/{categoryId}/products")]
		public IActionResult showProductsTest([FromRoute(Name = "categoryId")] int categoryId)
		{
			return View("ProductsView", _database.Products.Include(e => e.Category).Where(e => e.CategoryId == categoryId).ToList());
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
