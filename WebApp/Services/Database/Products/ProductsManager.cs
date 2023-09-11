using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Database.Models;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Maintenance;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Filtering;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Database.Products
{
	public class ProductsManager
	{
		private readonly DatabaseContext _database;
		private readonly ProductImagesManager _images;
		private readonly BrandsManager _brands;
		private readonly CategoriesManager _categories;
		private readonly ColoursManager _colours;
		private readonly SizesManager _sizes;
		private readonly UserInteractionManager _interactions;

		private async Task ValidateCategoryId(int categoryId)
		{
			if (!await _categories.CheckIfLastAsync(categoryId))
			{
				throw new UserInteractionException("Invalid category provided. Reload the page.");
			}
		}

		private async Task<Product> AddNewProductAsync(int ownerId, ProductCreate vm)
		{
			await ValidateCategoryId(vm.CategoryId);
			Product newProduct = new Product()
			{
				ProductOwnerId = ownerId,
				Name = vm.Name,
				Description = vm.Description,
				Price = vm.Price,
				Discount = vm.Discount,
				CategoryId = vm.CategoryId,
				BrandId = vm.BrandId == 0 ? null : vm.BrandId
			};

			_database.Products.Add(newProduct);
			await _database.SaveChangesAsync();

			return newProduct;
		}
		private async Task ChangeProductAsync(ProductUpdate vm)
		{
			await ValidateCategoryId(vm.CategoryId);
			Product updateProduct = await FindProductAsync(vm.Id);

			updateProduct.Id = vm.Id;
			updateProduct.Name = vm.Name;
			updateProduct.Description = vm.Description;
			updateProduct.Price = vm.Price;
			updateProduct.Discount = vm.Discount;
			updateProduct.CategoryId = vm.CategoryId;
			updateProduct.BrandId = vm.BrandId == 0 ? null : vm.BrandId;

			await _database.SaveChangesAsync();
		}

		private Task<List<ProductPreview>> PerformSearchAsync(
			List<IFilter<Product>> filters,
			IOrdering<Product> paginator,
			int pageSize)
		{
			IQueryable<Product> request = _database.Products
				.AsNoTracking()
				.Include(e => e.Stocks)
					.ThenInclude(e => e.Colour)
				.Include(e => e.Stocks)
					.ThenInclude(e => e.Size)
				.Where(e => e.Stocks.Count > 0);

			request = paginator.Apply(request);
			foreach (IFilter<Product> filter in filters)
			{
				request = filter.Apply(request);
			}

			//Mapping Product to ProductShowLightWeight is done in memory. Here we take only pageSize elements from database (and all their stocks information) I don't think that this result would take up THAT much memory.
			return request
				.Take(pageSize)
				.ToListAsync()
				.ContinueWith(next =>
					next.Result
						.Select(e => new ProductPreview()
						{
							Id = e.Id,
							Name = e.Name,
							Price = Math.Round(e.Price, 2),
							Discount = e.Discount,
							TruePrice = Math.Round(e.TruePrice, 2),
							TrueRating = e.TrueRating,
							ViewsCount = e.ViewsCount,
							MainImageId = e.MainImageId ?? 0,
							Date = e.Created,
							AvailableColours = e.Stocks
								.DistinctBy(e => e.ColourId)
								.Select(e => new ColourModel()
								{
									Id = e.Colour.Id,
									Name = e.Colour.Name,
									HexCode = e.Colour.HexCode
								})
								.ToList(),
							AvailableSizes = e.Stocks
								.DistinctBy(e => e.SizeId)
								.Select(e => new SizeModel()
								{
									Id = e.Size.Id,
									Name = e.Size.SizeName
								})
								.ToList()
						})
						.ToList()
			);
		}

		public ProductsManager(
			DatabaseContext database,
			ProductImagesManager images,
			BrandsManager brands,
			CategoriesManager categories,
			ColoursManager colours,
			SizesManager sizes,
			UserInteractionManager interactions)
		{
			_database = database;
			_images = images;
			_brands = brands;
			_categories = categories;
			_colours = colours;
			_sizes = sizes;
			_interactions = interactions;
		}

		public async Task<Product> FindProductAsync(int productId)
		{
			return await _database.Products.FindAsync(productId) ??
				throw new UserInteractionException(
					string.Format("Product with id {0} does not exist.", productId));
		}
		public async Task<int> GetProductMainImage(int productId)
		{
			return await _database.Products
				.Where(e => e.Id == productId)
				.Select(e => e.MainImageId)
				.FirstAsync() ?? throw new UserInteractionException(
					string.Format("Product with id {0} does not exist.", productId));
		}

		public async Task<ProductShow> GetProductShowVMAsync(int actionPerformer, int productId)
		{
			Product foundProduct = await _database.Products
				.Include(e => e.Brand)
				.Include(e => e.ProductOwner)
				.FirstOrDefaultAsync(e => e.Id == productId) ?? throw new UserInteractionException(
					string.Format("Product with id {0} does not exist.", productId));

			if (foundProduct.ProductOwnerId != actionPerformer)
			{
				bool firstTime =
					await _interactions.RememberViewedProductAsync(actionPerformer, productId); ;

				if (firstTime)
				{
					foundProduct.ViewsCount += 1;
					await _database.SaveChangesAsync();
				}
			}

			return new ProductShow()
			{
				Id = foundProduct.Id,
				DisplayEditing = actionPerformer == foundProduct.ProductOwnerId,
				AuthorName = foundProduct.ProductOwner.UserName,
				Name = foundProduct.Name,
				Description = foundProduct.Description,
				Price = Math.Round(foundProduct.Price, 2),
				Discount = foundProduct.Discount,
				TruePrice = Math.Round(foundProduct.TruePrice, 2),
				ViewsCount = foundProduct.ViewsCount,
				Rating = foundProduct.TrueRating,
				ReviewsCount = foundProduct.RatingsCount,
				BrandInfo = foundProduct.Brand == null
					? null
					: (foundProduct.Brand.Name, foundProduct.Brand.ImageId ?? 0),
				MainImageId = foundProduct.MainImageId ?? 0,
				ProductImagesIds = await _images.GetProductImagesAsync(productId),
				AvailableColours = await _colours.GetAllColoursAsync(),
				AvailableSizes = await _sizes.GetAllSizesAsync()
			};
		}
		public async Task<ProductUpdate> GetProductUpdateVMAsync(int productId)
		{
			Product foundProduct = await FindProductAsync(productId);
			return new ProductUpdate()
			{
				Id = foundProduct.Id,
				Name = foundProduct.Name,
				Description = foundProduct.Description,
				Price = foundProduct.Price,
				Discount = foundProduct.Discount,
				BrandId = foundProduct.BrandId ?? 0,
				CategoryId = foundProduct.CategoryId,
				AvailableCategories =
					await _categories.GetSelectListWithSelectedIdAsync(foundProduct.CategoryId),
				AvailableBrands = foundProduct.BrandId == null
					? await _brands.GetSelectListAsync()
					: await _brands.GetSelectListWithSelectedIdAsync(foundProduct.BrandId.Value)
			};
		}
		public async Task<ProductCreate> GetProductCreateVMAsync()
		{
			return new ProductCreate()
			{
				AvailableCategories = await _categories.GetSelectListAsync(),
				AvailableBrands = await _brands.GetSelectListAsync()
			};
		}

		public Task<List<ProductPreview>> SearchAsync(
			List<IFilter<Product>> filters,
			IOrdering<Product> paginator)
		{
			return PerformSearchAsync(filters, paginator, 8);
		}

		public async Task<Product> CreateProductAsync(int ownerId, ProductCreate vm)
		{
			using (var transaction = _database.Database.BeginTransaction())
			{
				Product createdProduct = await AddNewProductAsync(ownerId, vm);
				if (vm.ProductImages != null)
				{
					List<ProductImage> loadedImages = await _images.AddImagesToProductAsync(createdProduct.Id, vm.ProductImages);
					createdProduct.MainImageId = loadedImages[0].Id;

					await _database.SaveChangesAsync();
				}
				else
				{
					throw new UserInteractionException("Для створення нового товару потрібно додати хоча б одне зображення.");
				}

				transaction.Commit();
				return createdProduct;
			}
		}
		public async Task UpdateProductAsync(ProductUpdate vm)
		{
			using (var transaction = _database.Database.BeginTransaction())
			{
				await ChangeProductAsync(vm);
				if (vm.ProductImages != null)
					await _images.AddImagesToProductAsync(vm.Id, vm.ProductImages);

				transaction.Commit();
			}
		}
		public async Task DeleteProductAsync(int productId)
		{
			_database.Products.Remove(
				await FindProductAsync(productId)
			);

			await _database.SaveChangesAsync();
		}

		public async Task ChangeMainImage(int productId, int newMainImageId)
		{
			Product foundProduct = await FindProductAsync(productId);
			foundProduct.MainImageId = newMainImageId;

			await _database.SaveChangesAsync();
		}
		public async Task RateProduct(int actionPerformer, int productId, int stars)
		{
			if (stars > ProductShow.MaxStarsRating)
				throw new UserInteractionException("Maximum rating for the product is: " + ProductShow.MaxStarsRating);

			bool firstTime =
				await _interactions.RememberRatedProductAsync(actionPerformer, productId);

			if (firstTime)
			{
				Product foundProduct = await FindProductAsync(productId);
				foundProduct.StarsCount += stars;
				foundProduct.RatingsCount += 1;

				await _database.SaveChangesAsync();
			}
			else
			{
				throw new UserInteractionException("Ви вже оцінювали цей продукт.");
			}
		}
	}
}
