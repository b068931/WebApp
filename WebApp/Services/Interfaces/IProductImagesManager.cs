using WebApp.Database.Entities;

namespace WebApp.Services.Interfaces
{
	public interface IProductImagesManager
	{
		List<Image> AddImagesToProduct(int productId, List<IFormFile> images);
		void DeleteImages(List<int> imagesToDeleteIds);

		List<int> GetProductImages(int productId);
		Task<Image> FindImage(int id);
	}
}
