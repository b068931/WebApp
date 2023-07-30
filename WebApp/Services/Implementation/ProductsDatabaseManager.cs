using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Helpers;
using WebApp.Services.Interfaces;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Implementation
{
	public class ProductsDatabaseManager : IProductsManager
	{
		private readonly DatabaseContext _database;
		private readonly IProductImagesManager _images;
		private readonly IBrandsManager _brands;
		private readonly ICategoriesManager _categories;

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
		private void ChangeProduct(ProductUpdate vm)
		{
			Product updateProduct = new Product()
			{
				Id = vm.Id,
				Name = vm.Name,
				Description = vm.Description,
				Price = vm.Price,
				Discount = vm.Discount,
				CategoryId = vm.CategoryId,
				BrandId = (vm.BrandId == 0) ? null : vm.BrandId
			};

			_database.Products.Update(updateProduct);
			_database.Products
				.Entry(updateProduct)
				.Property(e => e.MainImageId)
				.IsModified = false;

			_database.SaveChanges();
		}

		public ProductsDatabaseManager(
			DatabaseContext database,
			IProductImagesManager images,
			IBrandsManager brands,
			ICategoriesManager categories)
		{
			_database = database;
			_images = images;
			_brands = brands;
			_categories = categories;
		}

		public Product FindProduct(int productId)
		{
			return _database.Products.Find(productId) ??
				throw new UserInteractionException(
					string.Format("Product with id {0} does not exist.", productId));
		}
		public int GetProductMainImage(int productId)
		{
			return _database.Products
				.Where(e => e.Id == productId)
				.Select(e => e.MainImageId)
				.First() ?? throw new UserInteractionException(
					string.Format("Product with id {0} does not exist.", productId));
		}

		public ProductShow GetProductShowVM(int productId)
		{
			Product foundProduct = FindProduct(productId);
			_database.Entry(foundProduct).Reference(e => e.Brand).Load();

			return new ProductShow()
			{
				Id = foundProduct.Id,
				Name = foundProduct.Name,
				Description = foundProduct.Description,
				Price = foundProduct.Price,
				Discount = foundProduct.Discount,
				BrandName = (foundProduct.Brand == null) ? null : foundProduct.Brand.Name,
				MainImageId = foundProduct.MainImageId ?? 0,
				ProductImagesIds = _images.GetProductImages(productId)
			};
		}
		public ProductUpdate GetProductUpdateVM(int productId)
		{
			Product foundProduct = FindProduct(productId);
			return new ProductUpdate()
			{
				Id = foundProduct.Id,
				Name = foundProduct.Name,
				Description = foundProduct.Description,
				Price = foundProduct.Price,
				Discount = foundProduct.Discount,
				BrandId = foundProduct.BrandId ?? 0,
				CategoryId = foundProduct.CategoryId,
				AvailableCategories = _categories.GetSelectListWithSelectedId(foundProduct.CategoryId),
				AvailableBrands = (foundProduct.BrandId == null) ? _brands.GetSelectList() : _brands.GetSelectListWithSelectedId(foundProduct.BrandId.Value)
			};
		}
		public ProductCreate GetProductCreateVM()
		{
			return new ProductCreate()
			{
				AvailableCategories = _categories.GetSelectList(),
				AvailableBrands = _brands.GetSelectList()
			};
		}

		public Product CreateProduct(ProductCreate vm)
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
				return createdProduct;
			}
		}
		public void UpdateProduct(ProductUpdate vm)
		{
			using (var transaction = _database.Database.BeginTransaction())
			{
				ChangeProduct(vm);
				if(vm.ProductImages != null) 
					_images.AddImagesToProduct(vm.Id, vm.ProductImages);

				transaction.Commit();
			}
		}
		public void DeleteProduct(int productId)
		{
			Product foundProduct = FindProduct(productId);
			_database.Products.Remove(foundProduct);

			_database.SaveChanges();
		}
		public void ChangeMainImage(int productId, int newMainImageId)
		{
			Product foundProduct = FindProduct(productId);
			foundProduct.MainImageId = newMainImageId;

			_database.SaveChanges();
		}
	}
}
