using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Services.Interfaces;

namespace WebApp.Services.Implementation
{
	public class BrandsDatabaseManager : IBrandsManager
	{
		private readonly DatabaseContext _database;
		private Brand FindBrand(int id)
		{
			return _database.Brands.Find(id) ?? throw new ArgumentOutOfRangeException(string.Format("Brand with id {0} does not exist.", id));
		}

		public BrandsDatabaseManager(DatabaseContext database)
		{
			_database = database;
		}

		public List<Database.Models.Brand> GetAllBrands()
		{
			return _database.Brands
					.AsNoTracking()
					.Select(e => new Database.Models.Brand() { Id = e.Id, Name = e.Name })
					.ToList();
		}
		public List<SelectListItem> GetSelectList() 
		{
			return _database.Brands
				.AsNoTracking()
				.Select(e => new SelectListItem() { Value = e.Id.ToString(), Text = e.Name })
				.ToList();
		}

		public void CreateBrand(string newBrandName)
		{
			_database.Brands.Add(new Brand { Name = newBrandName });
			_database.SaveChanges();
		}
		public void DeleteBrand(int id)
		{
			_database.Brands.Remove(FindBrand(id));
			_database.SaveChanges();
		}
		public void RenameBrand(int id, string newName)
		{
			Brand foundBrand = FindBrand(id);

			foundBrand.Name = newName;
			_database.SaveChanges();
		}
	}
}
