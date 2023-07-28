using WebApp.Database.Entities;

namespace WebApp.Services.Interfaces
{
	public interface IProductImagesManager
	{
		List<Image> AddImagesToProduct(int productId, List<IFormFile> images);
		Image FindImage(int id);
	}
}
