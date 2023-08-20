using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities;
using WebApp.Database.Models;
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

		private void ValidateCategoryId(int categoryId)
		{
			if (!_categories.CheckIfLast(categoryId))
			{
				throw new UserInteractionException("Invalid category provided. Reload the page.");
			}
		}

		private Product AddNewProduct(ProductCreate vm)
		{
			ValidateCategoryId(vm.CategoryId);
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
			ValidateCategoryId(vm.CategoryId);
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

			_database.Products
				.Entry(updateProduct)
				.Property(e => e.Created)
				.IsModified = false;

			_database.SaveChanges();
		}

		private List<ProductShowLightWeightJson> PerformSearch(
			List<IFilter<Product>> filters,
			IOrdering<Product> paginator,
			int pageSize)
		{
			IQueryable<Product> request = _database.Products
				.AsNoTracking()
				.Include(e => e.Brand);

			request = paginator.Apply(request);
			foreach (IFilter<Product> filter in filters)
			{
				request = filter.Apply(request);
			}

			return request
			   .Select(e => new ProductShowLightWeightJson()
			   {
				   Id = e.Id,
				   Name = e.Name,
				   Description = e.Description,
				   Price = e.Price,
				   Discount = e.Discount,
				   Stars = e.Stars,
				   Date = e.Created,
				   ViewsCount = e.ViewsCount,
				   MainImageId = e.MainImageId ?? 0
			   })
			   .Take(pageSize)
			   .ToList();
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
				BrandInfo = (foundProduct.Brand == null) 
					? null 
					: (foundProduct.Brand.Name, foundProduct.Brand.ImageId ?? 0),
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

		public List<ProductShowLightWeightJson> Search(
			List<IFilter<Product>> filters,
			IOrdering<Product> paginator)
		{
			return PerformSearch(filters, paginator, 1);
		}

		public Product CreateProduct(ProductCreate vm)
		{
			using (var transaction = _database.Database.BeginTransaction())
			{
				Product createdProduct = AddNewProduct(vm);
				if (vm.ProductImages != null)
				{
					List<ProductImage> loadedImages = _images.AddImagesToProduct(createdProduct.Id, vm.ProductImages);
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
