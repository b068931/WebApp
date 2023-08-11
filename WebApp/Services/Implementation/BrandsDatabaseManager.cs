using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers;
using WebApp.Services.Interfaces;

namespace WebApp.Services.Implementation
{
	public class BrandsDatabaseManager : IBrandsManager
	{
		private static readonly int MaxImageSize = 10485760;

		private readonly DatabaseContext _database;
		
		private Brand FindBrand(int id)
		{
			return _database.Brands.Find(id) ?? throw new ArgumentOutOfRangeException(string.Format("Brand with id {0} does not exist.", id));
		}
		private BrandImage FindBrandImage(int id)
		{
			return _database.BrandImages.Find(id) ?? throw new ArgumentOutOfRangeException(string.Format("Brand image with id {0} does not exist.", id));
		}
		private BrandImage GenerateBrandImageFromFile(IFormFile brandImageFile)
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
				brandImageFile.CopyTo(memory);
				brandImage.Data = memory.ToArray();
			}

			return brandImage;
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
		public List<SelectListItem> GetSelectListWithSelectedId(int brandId)
		{
			List<SelectListItem> brands = GetSelectList();
			foreach (var brand in brands)
			{
				if(brand.Value == brandId.ToString())
				{
					brand.Selected = true;
					break;
				}
			}

			return brands;
		}

		public void CreateBrand(string newBrandName, IFormFile brandImageFile)
		{
			Brand newBrand = new Brand { Name = newBrandName };
			newBrand.Image = GenerateBrandImageFromFile(brandImageFile);

			_database.Brands.Add(newBrand);
			_database.SaveChanges();
		}
		public void UpdateBrand(int id, string newName, IFormFile? brandImage)
		{
			Brand foundBrand = FindBrand(id);
			using (var transaction = _database.Database.BeginTransaction()) {
				if (brandImage != null)
				{
					BrandImage newImage = GenerateBrandImageFromFile(brandImage);
					_database.BrandImages.Add(newImage);
					_database.SaveChanges();

					BrandImage previousImage = new BrandImage() { Id = foundBrand.ImageId ?? throw new ArgumentNullException("This should not happen.") };
					_database.BrandImages.Attach(previousImage);
					_database.BrandImages.Remove(previousImage);
					_database.SaveChanges();

					foundBrand.ImageId = newImage.Id;
				}

				foundBrand.Name = newName;
				_database.SaveChanges();

				transaction.Commit();
			}
		}
		public void DeleteBrand(int id)
		{
			_database.Brands.Remove(FindBrand(id));
			_database.SaveChanges();
		}
		public async Task<BrandImage> GetBrandImage(int brandImageId)
		{
			return await _database.BrandImages.FindAsync(brandImageId) ??
				throw new ArgumentOutOfRangeException(
					string.Format("Image with id {0} does not exist. (BrandImage)", brandImageId));
		}
	}
}