using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using WebApp.Database;
using WebApp.Database.Entities;

namespace WebApp.Controllers
{
	[Route("/brands")]
	public class BrandsController : Controller
	{
		private readonly DatabaseContext _database;
		private readonly ILogger<BrandsController> _logger;
		
		private List<Database.Models.Brand> GetAllBrands()
		{
			return _database.Brands
					.AsNoTracking()
					.Select(e => new Database.Models.Brand() { Id = e.Id, Name = e.Name })
					.ToList();
		}

		private IActionResult PerformAction(Action callback)
		{
			try
			{
				callback();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Function is so simple yet managed to fail.");
			}

			return Redirect("/brands");
		}
		private Brand FindBrand(int id)
		{
			return _database.Brands.Find(id) ?? throw new ArgumentOutOfRangeException(string.Format("Brand with id {0} does not exist.", id));
		}

		private void CreateNewBrand(string newBrandName)
		{
			_database.Brands.Add(new Brand { Name = newBrandName });
			_database.SaveChanges();
		}
		private void RenameBrand(int id, string newName)
		{
			Brand foundBrand = FindBrand(id);

			foundBrand.Name = newName;
			_database.SaveChanges();
		}
		private void DeleteBrand(int id)
		{
			_database.Brands.Remove(FindBrand(id));
			_database.SaveChanges();
		}

		public BrandsController(DatabaseContext database, ILogger<BrandsController> logger)
		{
			_database = database;
			_logger = logger;
		}

		[HttpGet("/brands/json")]
		public JsonResult GetAll()
		{
			return Json(
				GetAllBrands(),
				new JsonSerializerOptions(JsonSerializerDefaults.Web)
			);
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			[FromForm(Name = "brandName")] string newBrandName)
		{
			return PerformAction(() => CreateNewBrand(newBrandName));
		}

		[HttpPost("action/rename")]
		[ValidateAntiForgeryToken]
		public IActionResult Rename(
			[FromForm(Name = "brandId")] int idToRename,
			[FromForm(Name = "brandName")] string newName)
		{
			return PerformAction(() => RenameBrand(idToRename, newName));
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "brandId")] int idToDelete)
		{
			return PerformAction(() =>  DeleteBrand(idToDelete));
		}

		public IActionResult Index()
		{
			return View("AdminPage", GetAllBrands());
		}
	}
}
