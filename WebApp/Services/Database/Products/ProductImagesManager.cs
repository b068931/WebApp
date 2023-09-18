using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Database.Models;
using WebApp.Utilities.Exceptions;

namespace WebApp.Services.Database.Products
{
	public class ProductImagesManager
	{
		public const int MaxImagesCount = 8;
		private const int MaxFileSize = 4194304;
		private const string ImagesStorageRelativePath = "images/";

		private readonly DatabaseContext _database;
		private readonly IWebHostEnvironment _environment;

		private void ValidateImages(List<IFormFile> images)
		{
			foreach (var imageFile in images)
			{
				if (imageFile.Length > MaxFileSize)
				{
					throw new UserInteractionException(
						string.Format(
							"Файл {0} занадто великий. Максимальний розмір файлу - {1} байт.",
							imageFile.FileName, MaxFileSize
						)
					);
				}
			}
		}
		private async Task<List<ProductImage>> CreateImageEntitiesAsync(int productId, List<IFormFile> images)
		{
			ValidateImages(images);

			List<ProductImage> newImages = new List<ProductImage>();
			foreach (var imageFile in images)
			{
				string imageName = Path.ChangeExtension(
					Path.Combine(ImagesStorageRelativePath, Guid.NewGuid().ToString()),
					Path.GetExtension(imageFile.FileName)
				);

				ProductImage newImage = new ProductImage()
				{
					ProductId = productId,
					StorageRelativeLocation = "/" + imageName
				};

				string fileName = Path.Combine(
					_environment.WebRootPath,
					imageName
				);

				using (var destination = File.Create(fileName))
				{
					await imageFile.CopyToAsync(destination);
				}

				newImages.Add(newImage);
			}

			return newImages;
		}

		public ProductImagesManager(
			DatabaseContext database, 
			IWebHostEnvironment environment)
		{
			_database = database;
			_environment = environment;
		}

		public async Task<List<ProductImage>> AddImagesToProductAsync(int productId, List<IFormFile> images)
		{
			int imagesCount = await GetProductImagesCountAsync(productId) + images.Count;
			if (imagesCount > MaxImagesCount)
				throw new UserInteractionException(
					string.Format(
						"Максимальна кількість зображень для одного продукту: {0}.",
						MaxImagesCount
					)
				);

			List<ProductImage> loadedImages = await CreateImageEntitiesAsync(productId, images);

			_database.ProductImages.AddRange(loadedImages);
			await _database.SaveChangesAsync();

			return loadedImages;
		}
		public async Task DeleteImagesAsync(int productId, List<int> imagesToDeleteIds)
		{
			List<ProductImage> productImages = await _database.ProductImages
				.Where(e => imagesToDeleteIds.Contains(e.Id))
				.Where(e => e.ProductId == productId)
				.ToListAsync();

			foreach(var image in productImages)
			{
				File.Delete(
					_environment.WebRootPath + image.StorageRelativeLocation
				);
			}

			_database.ProductImages.RemoveRange(productImages);
			await _database.SaveChangesAsync();
		}

		public Task<List<ProductImageModel>> GetProductImagesAsync(int productId)
		{
			return _database.ProductImages
				.Where(e => e.ProductId == productId)
				.Select(e => new ProductImageModel()
				{
					Id = e.Id,
					Path = e.StorageRelativeLocation
				})
				.ToListAsync();
		}
		public Task<int> GetProductImagesCountAsync(int productId)
		{
			return _database.Products
				.Include(e => e.Images)
				.Where(e => e.Id == productId)
				.Select(e => e.Images.Count)
				.FirstAsync();
		}
	}
}