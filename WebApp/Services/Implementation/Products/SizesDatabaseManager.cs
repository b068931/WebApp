using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers;
using WebApp.Services.Interfaces.Products;

namespace WebApp.Services.Implementation.Products
{
	public class SizesDatabaseManager : ISizesManager
	{
		private readonly DatabaseContext _database;
		Size FindSize(int id)
		{
			return _database.ProductSizes.Find(id) ??
				throw new UserInteractionException(
					string.Format("Size with id {0} does not exist.", id)
				);
		}

		public SizesDatabaseManager(DatabaseContext database)
		{
			_database = database;
		}

		public List<Database.Models.Size> GetAllSizes()
		{
			return _database.ProductSizes
				.AsNoTracking()
				.Select(e => new Database.Models.Size()
				{
					Id = e.Id,
					Name = e.SizeName
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
