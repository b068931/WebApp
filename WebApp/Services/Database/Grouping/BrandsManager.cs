using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Grouping;
using WebApp.Database.Models;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Grouping
{
	public class BrandsManager
	{
		private static readonly int MaxImageSize = 10485760;

		private readonly DatabaseContext _database;

		private async Task<Brand> FindBrandAsync(int id)
		{
			return await _database.Brands.FindAsync(id)
				?? throw new ArgumentOutOfRangeException(
					$"Brand with id {id} does not exist."
				);
		}
		private static async Task<BrandImage> GenerateBrandImageFromFileAsync(IFormFile brandImageFile)
		{
			if (brandImageFile.Length > MaxImageSize)
			{
				throw new UserInteractionException(
					$"Файл {brandImageFile.FileName} занадто великий. Максимальний розмір файлу - {MaxImageSize}."
				);
			}

			BrandImage brandImage = new()
			{
				ContentType = brandImageFile.ContentType
			};

			using (var memory = new MemoryStream())
			{
				await brandImageFile.CopyToAsync(memory);
				brandImage.Data = memory.ToArray();
			}

			return brandImage;
		}

		public BrandsManager(DatabaseContext database)
		{
			_database = database;
		}

		public Task<List<BrandModel>> GetAllBrandsAsync()
		{
			return _database.Brands
					.AsNoTracking()
					.Select(e => new BrandModel()
					{
						Id = e.Id,
						Name = e.Name,
						ImageId = e.ImageId ?? 0
					})
					.ToListAsync();
		}
		public Task<List<SelectListItem>> GetSelectListAsync()
		{
			return _database.Brands
				.AsNoTracking()
				.Select(e => new SelectListItem() { Value = e.Id.ToString(), Text = e.Name })
				.ToListAsync();
		}
		public Task<List<SelectListItem>> GetSelectListWithSelectedIdAsync(int brandId)
		{
			return GetSelectListAsync()
			.ContinueWith(next =>
			{
				var brands = next.Result;
				foreach (var brand in brands)
				{
					if (brand.Value == brandId.ToString())
					{
						brand.Selected = true;
						break;
					}
				}

				return brands;
			});
		}

		public async Task CreateBrandAsync(string newBrandName, IFormFile brandImageFile)
		{
			Brand newBrand = new()
			{
				Name = newBrandName,
				Image = await GenerateBrandImageFromFileAsync(brandImageFile)
			};

			_database.Brands.Add(newBrand);
			await _database.SaveChangesAsync();
		}
		public async Task UpdateBrandAsync(int id, string newName, IFormFile? brandImage)
		{
			Brand foundBrand = await FindBrandAsync(id);
			if (brandImage != null)
			{
				BrandImage newImage = await GenerateBrandImageFromFileAsync(brandImage);

				newImage.Id = foundBrand.ImageId
					?? throw new ArgumentNullException(nameof(foundBrand.ImageId), "Unexpected null imageId for brand.");

				_database.BrandImages.Update(newImage);
			}

			foundBrand.Name = newName;
			await _database.SaveChangesAsync();
		}
		public async Task DeleteBrandAsync(int id)
		{
			_database.Brands.Remove(
				await FindBrandAsync(id)
			);

			await _database.SaveChangesAsync();
		}

		public async Task<BrandImageModel> GetBrandImageAsync(int brandImageId)
		{
			return await _database.BrandImages
				.Where(e => e.Id == brandImageId)
				.Select(e => new BrandImageModel()
				{
					ContentType = e.ContentType,
					ImageData = e.Data
				})
				.SingleOrDefaultAsync() ??
				throw new ArgumentOutOfRangeException(
					$"Brand image with id {brandImageId} does not exist."
				);
		}
		public async Task<int> GetBrandImageIdByBrandIdAsync(int brandId)
		{
			return await _database.Brands
				.Where(e => e.Id == brandId)
				.Select(e => e.ImageId)
				.SingleOrDefaultAsync()
					?? throw new UserInteractionException($"Brand with id {brandId} does not exist.");
		}
	}
}