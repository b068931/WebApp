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
		public const int MaxProductsPerUser = 12;

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

		private async Task<Product> CreateProductEntityAsync(int ownerId, ProductCreate vm)
		{
			await ValidateCategoryId(vm.CategoryId);
			Product newProduct = new()
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
					$"Product with id {productId} does not exist."
				);
		}
		public async Task<int> GetProductMainImage(int productId)
		{
			return await _database.Products
				.Where(e => e.Id == productId)
				.Select(e => e.MainImageId)
				.FirstOrDefaultAsync() ??
					throw new UserInteractionException(
						$"Product with id {productId} does not exist."
					);
		}

		public async Task<ProductShow> GetProductShowVMAsync(int actionPerformer, int productId)
		{
			Product foundProduct = await _database.Products
				.Include(e => e.Brand)
				.Include(e => e.MainImage)
				.Include(e => e.ProductOwner)
				.FirstOrDefaultAsync(e => e.Id == productId)
					?? throw new UserInteractionException(
						$"Product with id {productId} does not exist."
					);

			if (actionPerformer != 0 && foundProduct.ProductOwnerId != actionPerformer)
			{
				bool firstTime =
					await _interactions.RememberViewedProductAsync(actionPerformer, productId);

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
				MainImage = foundProduct.MainImage?.StorageRelativeLocation ?? "",
				ProductImages = await _images.GetProductImagesAsync(productId),
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
				AvailableImagesCount = ProductImagesManager.MaxImagesCount - await _images.GetProductImagesCountAsync(productId),
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
			IOrdering<Product> paginator,
			int pageSize,
			bool includeOnlyAvailable = true)
		{
			IQueryable<Product> request = _database.Products
				.AsNoTracking()
				.Include(e => e.MainImage)
				.Include(e => e.Stocks)
					.ThenInclude(e => e.Colour)
				.Include(e => e.Stocks)
					.ThenInclude(e => e.Size);

			if (includeOnlyAvailable)
				request = request.Where(e => e.Stocks.Count > 0);

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
							MainImage = e.MainImage?.StorageRelativeLocation ?? "",
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

		public async Task<Product> CreateProductAsync(int ownerId, ProductCreate vm)
		{
			int hasProducts = await _database.Products
				.Where(e => e.ProductOwnerId == ownerId)
				.CountAsync();

			if (hasProducts + 1 > MaxProductsPerUser)
				throw new UserInteractionException($"Ліміт продуктів для одного користувача: {MaxProductsPerUser}");

			await using var transaction = await _database.Database.BeginTransactionAsync();
			Product newProduct = await CreateProductEntityAsync(ownerId, vm);
			if (vm.ProductImages != null)
			{
				List<ProductImage> images =
					await _images.AddImagesToProductAsync(newProduct.Id, vm.ProductImages);

				newProduct.MainImageId = newProduct.Images[0].Id;
				await _database.SaveChangesAsync();
			}
			else
			{
				throw new UserInteractionException("Для створення нового товару потрібно додати хоча б одне зображення.");
			}

			await transaction.CommitAsync();
			return newProduct;
		}
		public async Task UpdateProductAsync(ProductUpdate vm)
		{
			await using var transaction = await _database.Database.BeginTransactionAsync();

			await ChangeProductAsync(vm);
			if (vm.ProductImages != null)
				await _images.AddImagesToProductAsync(vm.Id, vm.ProductImages);

			await transaction.CommitAsync();
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
			if (stars <= 0 || stars > ProductShow.MaxStarsRating)
				throw new UserInteractionException("Оцінка продукту має бути у межах від 0 до " + ProductShow.MaxStarsRating);

			Product foundProduct = await FindProductAsync(productId);
			if (foundProduct.ProductOwnerId == actionPerformer)
				throw new UserInteractionException("Ви не можете оцінити свій продукт.");

			bool firstTime =
				await _interactions.RememberRatedProductAsync(actionPerformer, productId);

			if (firstTime)
			{
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
