using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Interfaces;

namespace WebApp.Services.Implementation
{
	public class ProductImagesDatabaseManager : IProductImagesManager
	{
		private static readonly int MaxFileSize = 10485760;
		private readonly DatabaseContext _database;

		private void ValidateImages(List<IFormFile> images)
		{
			foreach (var imageFile in images)
			{
				if (imageFile.Length > MaxFileSize)
				{
					throw new ProductInteractionException(
						string.Format(
							"Файл {0} занадто великий. Максимальний розмір файлу - {1} байт.",
							imageFile.FileName, MaxFileSize
						)
					);
				}
			}
		}
		private List<Image> CreateImageEntities(int productId, List<IFormFile> images)
		{
			List<Image> newImages = new List<Image>();
			foreach (var imageFile in images)
			{
				Image newImage = new Image()
				{
					ProductId = productId,
					ContentType = imageFile.ContentType
				};

				using (var memory = new MemoryStream())
				{
					imageFile.CopyTo(memory);
					newImage.Data = memory.ToArray();
				}

				newImages.Add(newImage);
			}

			return newImages;
		}

		public ProductImagesDatabaseManager(DatabaseContext database)
		{
			_database = database;
		}

		public List<Image> AddImagesToProduct(int productId, List<IFormFile> images)
		{
			ValidateImages(images);
			List<Image> loadedImages = CreateImageEntities(productId, images);

			_database.Images.AddRange(loadedImages);
			_database.SaveChanges();

			return loadedImages;
		}
		public Image FindImage(int imageId)
		{
			return _database.Images.Find(imageId) ??
				throw new ProductInteractionException(
					string.Format("Image with id {0} does not exist.", imageId));
		}
	}
}
