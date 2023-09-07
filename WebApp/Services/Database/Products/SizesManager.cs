using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Products
{
	public class SizesManager
	{
		private readonly DatabaseContext _database;
		Size FindSize(int id)
		{
			return _database.ProductSizes.Find(id) ??
				throw new UserInteractionException(
					string.Format("Size with id {0} does not exist.", id)
				);
		}

		public SizesManager(DatabaseContext database)
		{
			_database = database;
		}

		public List<WebApp.Database.Models.Size> GetAllSizes()
		{
			return _database.ProductSizes
				.AsNoTracking()
				.Select(e => new WebApp.Database.Models.Size()
				{
					Id = e.Id,
					Name = e.SizeName
				})
				.ToList();
		}
		public List<SelectListItem> GetSelectList()
		{
			return _database.ProductSizes
				.AsNoTracking()
				.Select(e => new SelectListItem()
				{
					Text = e.SizeName,
					Value = e.Id.ToString()
				})
				.ToList();
		}

		public void CreateSize(string sizeName)
		{
			Size newSize = new Size()
			{
				SizeName = sizeName
			};

			_database.ProductSizes.Add(newSize);
			_database.SaveChanges();
		}
		public void UpdateSize(int id, string sizeName)
		{
			Size foundSize = FindSize(id);
			foundSize.SizeName = sizeName;

			_database.SaveChanges();
		}
		public void DeleteSize(int id)
		{
			_database.ProductSizes.Remove(FindSize(id));
			_database.SaveChanges();
		}
	}
}
