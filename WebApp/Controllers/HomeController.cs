using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApp.Database;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
	public class HomeController : Controller
	{
		//!!!THIS DATABASECONTEXT INSTANCE MUST BE REPLACED WITH IProductsManager WHEN THE DESIGN OF THE MAIN PAGE IS READY
		//RIGHT NOW IT IS USED TO QUERY ALL PRODUCTS AND SHOW THEM FOR DEBUG PURPOSES
		private readonly DatabaseContext _database;
		public HomeController(DatabaseContext database)
		{
			_database = database;
		}

		public IActionResult Index()
		{
			var result = _database.Products.Include(e => e.Brand).ToList();
            return View("MainPage", result);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
