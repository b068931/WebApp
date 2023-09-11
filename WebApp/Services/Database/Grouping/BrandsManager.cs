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

		private async Task<Brand> FindBrand(int id)
		{
			return await _database.Brands.FindAsync(id) ?? throw new ArgumentOutOfRangeException(string.Format("Brand with id {0} does not exist.", id));
		}
		private async Task<BrandImage> GenerateBrandImageFromFileAsync(IFormFile brandImageFile)
		{
			if (brandImageFile.Length > MaxImageSize)
			{
				throw new UserInteractionException(
					string.Format("Файл {0} занадто великий. Максимальний розмір файлу - {1}.", brandImageFile.FileName, MaxImageSize)
				);
			}

			BrandImage brandImage = new BrandImage();
			brandImage.ContentType = brandImageFile.ContentType;
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
			Brand newBrand = new Brand { Name = newBrandName };
			newBrand.Image = await GenerateBrandImageFromFileAsync(brandImageFile);

			_database.Brands.Add(newBrand);
			await _database.SaveChangesAsync();
		}
		public async Task UpdateBrandAsync(int id, string newName, IFormFile? brandImage)
		{
			Brand foundBrand = await FindBrand(id);
			using (var transaction = _database.Database.BeginTransaction())
			{
				if (brandImage != null)
				{
					BrandImage newImage = await GenerateBrandImageFromFileAsync(brandImage);
					_database.BrandImages.Add(newImage);
					await _database.SaveChangesAsync();

					BrandImage previousImage = new BrandImage() { Id = foundBrand.ImageId ?? throw new ArgumentNullException("This should not happen.") };
					_database.BrandImages.Attach(previousImage);
					_database.BrandImages.Remove(previousImage);
					await _database.SaveChangesAsync();

					foundBrand.ImageId = newImage.Id;
				}

				foundBrand.Name = newName;
				await _database.SaveChangesAsync();

				transaction.Commit();
			}
		}
		public async Task DeleteBrandAsync(int id)
		{
			_database.Brands.Remove(
				await FindBrand(id)
			);

			await _database.SaveChangesAsync();
		}
		public async Task<BrandImage> GetBrandImageAsync(int brandImageId)
		{
			return await _database.BrandImages.FindAsync(brandImageId) ??
				throw new ArgumentOutOfRangeException(
					string.Format("Image with id {0} does not exist. (BrandImage)", brandImageId));
		}
	}
}