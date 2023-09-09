﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Products
{
	public class SizesManager
	{
		private readonly DatabaseContext _database;
		private async Task<Size> FindSizeAsync(int id)
		{
			return await _database.ProductSizes.FindAsync(id) ??
				throw new UserInteractionException(
					string.Format("Size with id {0} does not exist.", id)
				);
		}

		public SizesManager(DatabaseContext database)
		{
			_database = database;
		}

		public Task<List<WebApp.Database.Models.Size>> GetAllSizesAsync()
		{
			return _database.ProductSizes
				.AsNoTracking()
				.Select(e => new WebApp.Database.Models.Size()
				{
					Id = e.Id,
					Name = e.SizeName
				})
				.ToListAsync();
		}
		public Task<List<SelectListItem>> GetSelectListAsync()
		{
			return _database.ProductSizes
				.AsNoTracking()
				.Select(e => new SelectListItem()
				{
					Text = e.SizeName,
					Value = e.Id.ToString()
				})
				.ToListAsync();
		}

		public Task CreateSizeAsync(string sizeName)
		{
			Size newSize = new Size()
			{
				SizeName = sizeName
			};

			_database.ProductSizes.Add(newSize);
			return _database.SaveChangesAsync();
		}
		public async Task UpdateSizeAsync(int id, string sizeName)
		{
			Size foundSize = await FindSizeAsync(id);
			foundSize.SizeName = sizeName;

			await _database.SaveChangesAsync();
		}
		public async Task DeleteSizeAsync(int id)
		{
			_database.ProductSizes.Remove(
				await FindSizeAsync(id)
			);

			await _database.SaveChangesAsync();
		}
	}
}
