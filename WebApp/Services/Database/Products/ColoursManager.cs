using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Database.Models;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Products
{
	public class ColoursManager
	{
		private readonly DatabaseContext _database;
		private async Task<Colour> FindColourAsync(int id)
		{
			return await _database.ProductColours.FindAsync(id) ??
					throw new UserInteractionException(
						string.Format("Colour with id {0} does not exist.", id)
					);
		}

		public ColoursManager(DatabaseContext database)
		{
			_database = database;
		}

		public Task<List<ColourModel>> GetAllColoursAsync()
		{
			return _database.ProductColours
				.AsNoTracking()
				.Select(e => new ColourModel
				{
					Id = e.Id,
					Name = e.Name,
					HexCode = e.HexCode
				})
				.ToListAsync();
		}
		public Task<List<SelectListItem>> GetSelectListAsync()
		{
			return _database.ProductColours
				.AsNoTracking()
				.Select(e => new SelectListItem
				{
					Text = e.Name,
					Value = e.Id.ToString()
				})
				.ToListAsync();
		}

		public Task CreateColourAsync(string name, string hexCode)
		{
			Colour newColour = new Colour()
			{
				Name = name,
				HexCode = hexCode
			};

			_database.ProductColours.Add(newColour);
			return _database.SaveChangesAsync();
		}
		public async Task UpdateColourAsync(int id, string name, string hexCode)
		{
			Colour foundColour = await FindColourAsync(id);
			foundColour.Name = name;
			foundColour.HexCode = hexCode;

			await _database.SaveChangesAsync();
		}
		public async Task DeleteColourAsync(int id)
		{
			_database.ProductColours.Remove(
				await FindColourAsync(id)
			);

			await _database.SaveChangesAsync();
		}
	}
}
