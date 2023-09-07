﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Products
{
	public class ColoursManager
	{
		private readonly DatabaseContext _database;
		private WebApp.Database.Entities.Products.Colour FindColour(int id)
		{
			return _database.ProductColours.Find(id) ??
					throw new UserInteractionException(
						string.Format("Colour with id {0} does not exist.", id)
					);
		}

		public ColoursManager(DatabaseContext database)
		{
			_database = database;
		}

		public List<WebApp.Database.Models.Colour> GetAllColours()
		{
			return _database.ProductColours
				.AsNoTracking()
				.Select(e => new WebApp.Database.Models.Colour
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
			WebApp.Database.Entities.Products.Colour newColour = new WebApp.Database.Entities.Products.Colour()
			{
				Name = name,
				HexCode = hexCode
			};

			_database.ProductColours.Add(newColour);
			_database.SaveChanges();
		}
		public void UpdateColour(int id, string name, string hexCode)
		{
			WebApp.Database.Entities.Products.Colour foundColour = FindColour(id);
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
