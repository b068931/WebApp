using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Interfaces;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Implementation
{
	public class ProductsDatabaseManager : IProductsManager
	{
		private readonly DatabaseContext _database;
		private readonly IProductImagesManager _images;

		private Product AddNewProduct(ProductCreate vm)
		{
			Product newProduct = new Product()
			{
				Name = vm.Name,
				Description = vm.Description,
				Price = vm.Price,
				Discount = vm.Discount,
				CategoryId = vm.CategoryId,
				BrandId = (vm.BrandId == 0) ? null : vm.BrandId
			};

			_database.Products.Add(newProduct);
			_database.SaveChanges();

			return newProduct;
		}

		public ProductsDatabaseManager(DatabaseContext database, IProductImagesManager images)
		{
			_database = database;
			_images = images;
		}

		public Product FindProduct(int productId)
		{
			return _database.Products.Find(productId) ??
				throw new ProductInteractionException(
					string.Format("Product with id {0} does not exist.", productId));
		}
		public void CreateProduct(ProductCreate vm)
		{
			using (var transaction = _database.Database.BeginTransaction())
			{
				Product createdProduct = AddNewProduct(vm);
				if (vm.ProductImages != null)
				{
					List<Image> loadedImages = _images.AddImagesToProduct(createdProduct.Id, vm.ProductImages);
					createdProduct.MainImageId = loadedImages[0].Id;

					_database.SaveChanges();
				}

				transaction.Commit();
			}
		}
	}
}
