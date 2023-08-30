using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Models;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Interfaces.Products;

namespace WebApp.Services.Implementation.Products
{
	public class ColoursDatabaseManager : IColoursManager
	{
		private readonly DatabaseContext _database;
		private Database.Entities.Colour FindColour(int id)
		{
			return _database.ProductColours.Find(id) ??
					throw new UserInteractionException(
						string.Format("Colour with id {0} does not exist.", id)
					);
		}

		public ColoursDatabaseManager(DatabaseContext database)
		{
			_database = database;
		}

		public List<Colour> GetAllColours()
		{
			return _database.ProductColours
				.AsNoTracking()
				.Select(e => new Colour 
				{ 
					Id = e.Id,
					Name = e.Name,
					HexCode = e.HexCode
				})
				.ToList();
		}
		public List<SelectListItem> GetSelectList()
		{
			return _database.ProductColours
				.AsNoTracking()
				.Select(e => new SelectListItem
				{
					Text = e.Name,
					Value = e.Id.ToString()
				})
				.ToList();
		}

		public void CreateColour(string name, string hexCode)
		{
			Database.Entities.Colour newColour = new Database.Entities.Colour()
			{
				Name = name,
				HexCode = hexCode
			};

			_database.ProductColours.Add(newColour);
			_database.SaveChanges();
		}
		public void UpdateColour(int id, string name, string hexCode)
		{
			Database.Entities.Colour foundColour = FindColour(id);
			foundColour.Name = name;
			foundColour.HexCode = hexCode;

			_database.SaveChanges();
		}
		public void DeleteColour(int id)
		{
			_database.ProductColours.Remove(FindColour(id));
			_database.SaveChanges();
		}
	}
}
