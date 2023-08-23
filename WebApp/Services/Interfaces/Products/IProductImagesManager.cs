using WebApp.Database.Entities;

namespace WebApp.Services.Interfaces.Products
{
    public interface IProductImagesManager
    {
        List<ProductImage> AddImagesToProduct(int productId, List<IFormFile> images);
        void DeleteImages(List<int> imagesToDeleteIds);

        List<int> GetProductImages(int productId);
        Task<ProductImage> FindImage(int id);
    }
}
