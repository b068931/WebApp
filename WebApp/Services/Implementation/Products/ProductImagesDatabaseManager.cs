using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Interfaces.Products;

namespace WebApp.Services.Implementation.Products
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
                    throw new UserInteractionException(
                        string.Format(
                            "Файл {0} занадто великий. Максимальний розмір файлу - {1} байт.",
                            imageFile.FileName, MaxFileSize
                        )
                    );
                }
            }
        }
        private List<ProductImage> CreateImageEntities(int productId, List<IFormFile> images)
        {
            List<ProductImage> newImages = new List<ProductImage>();
            foreach (var imageFile in images)
            {
                ProductImage newImage = new ProductImage()
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

        public List<ProductImage> AddImagesToProduct(int productId, List<IFormFile> images)
        {
            ValidateImages(images);
            List<ProductImage> loadedImages = CreateImageEntities(productId, images);

            _database.ProductImages.AddRange(loadedImages);
            _database.SaveChanges();

            return loadedImages;
        }
        public void DeleteImages(List<int> imagesToDeleteIds)
        {
            _database.ProductImages.RemoveRange(
                _database.ProductImages.Where(e => imagesToDeleteIds.Contains(e.Id))
            );

            _database.SaveChanges();
        }

        public List<int> GetProductImages(int productId)
        {
            return _database.ProductImages
                .Where(e => e.ProductId == productId)
                .Select(e => e.Id)
                .ToList();
        }
        public async Task<ProductImage> FindImage(int imageId)
        {
            return await _database.ProductImages.FindAsync(imageId) ??
                throw new ArgumentOutOfRangeException(
                    string.Format("Image with id {0} does not exist. (ProductImage)", imageId));
        }
    }
}
