﻿using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Products;
using WebApp.Database.Models;
using WebApp.Helpers.Exceptions;
using WebApp.Helpers.Filtering;
using WebApp.Services.Implementation.Grouping;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Implementation.Products
{
    public class ProductsManager
    {
        private readonly DatabaseContext _database;
        private readonly ProductImagesManager _images;
        private readonly BrandsManager _brands;
        private readonly CategoriesManager _categories;
        private readonly ColoursManager _colours;
        private readonly SizesManager _sizes;

        private void ValidateCategoryId(int categoryId)
        {
            if (!_categories.CheckIfLast(categoryId))
            {
                throw new UserInteractionException("Invalid category provided. Reload the page.");
            }
        }
        private Database.Entities.Products.ProductStock FindProductStock(int stockId)
        {
            return _database.ProductStocks.Find(stockId) ??
                throw new UserInteractionException(
                    string.Format("Product stock with id {0} does not exist", stockId)
                );
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
                BrandId = vm.BrandId == 0 ? null : vm.BrandId
            };

            _database.Products.Add(newProduct);
            _database.SaveChanges();

            return newProduct;
        }
        private void ChangeProduct(ProductUpdate vm)
        {
            ValidateCategoryId(vm.CategoryId);
            Product updateProduct = FindProduct(vm.Id);

            updateProduct.Id = vm.Id;
            updateProduct.Name = vm.Name;
            updateProduct.Description = vm.Description;
            updateProduct.Price = vm.Price;
            updateProduct.Discount = vm.Discount;
            updateProduct.CategoryId = vm.CategoryId;
            updateProduct.BrandId = vm.BrandId == 0 ? null : vm.BrandId;

            _database.SaveChanges();
        }

        private async Task<List<ProductShowLightWeight>> PerformSearchAsync(
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

            List<Product> searchResult = await request
               .Take(pageSize) //Mapping Product to ProductShowLightWeight is done in memory. Here we take only pageSize elements from database (and all their stocks information) I don't think that this result would take up THAT much memory.
               .ToListAsync();

            return searchResult
                .Select(e => new ProductShowLightWeight()
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
                        .Select(e => new Database.Models.Colour()
                        {
                            Id = e.Colour.Id,
                            Name = e.Colour.Name,
                            HexCode = e.Colour.HexCode
                        })
                        .ToList(),
                    AvailableSizes = e.Stocks
                        .DistinctBy(e => e.SizeId)
                        .Select(e => new Database.Models.Size()
                        {
                            Id = e.Size.Id,
                            Name = e.Size.SizeName
                        })
                        .ToList()
                })
                .ToList();
        }

        public ProductsManager(
            DatabaseContext database,
            ProductImagesManager images,
            BrandsManager brands,
            CategoriesManager categories,
            ColoursManager colours,
            SizesManager sizes)
        {
            _database = database;
            _images = images;
            _brands = brands;
            _categories = categories;
            _colours = colours;
            _sizes = sizes;
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
                ProductImagesIds = _images.GetProductImages(productId),
                AvailableColours = _colours.GetAllColours(),
                AvailableSizes = _sizes.GetAllSizes()
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
                AvailableBrands = foundProduct.BrandId == null ? _brands.GetSelectList() : _brands.GetSelectListWithSelectedId(foundProduct.BrandId.Value)
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

        public List<Database.Models.ProductStock> GetProductStocks(int productId)
        {
            return _database.ProductStocks
                .Include(e => e.Colour)
                .Include(e => e.Size)
                .Where(e => e.ProductId == productId)
                .Select(e => new Database.Models.ProductStock()
                {
                    Id = e.Id,
                    ProductAmount = e.ProductAmount,
                    Colour = new Database.Models.Colour()
                    {
                        Id = e.Colour.Id,
                        Name = e.Colour.Name,
                        HexCode = e.Colour.HexCode
                    },
                    Size = new Database.Models.Size()
                    {
                        Id = e.Size.Id,
                        Name = e.Size.SizeName
                    }
                })
                .ToList();
        }
        public void CreateProductStocks(int productId, int colourId, int sizeId, int stockSize)
        {
            Database.Entities.Products.ProductStock newStock = new Database.Entities.Products.ProductStock()
            {
                ProductAmount = stockSize,
                ProductId = productId,
                ColourId = colourId,
                SizeId = sizeId
            };

            _database.ProductStocks.Add(newStock);
            _database.SaveChanges();
        }
        public void UpdateProductStocks(int stockId, int colourId, int sizeId, int stockSize)
        {
            Database.Entities.Products.ProductStock foundStock = FindProductStock(stockId);
            foundStock.ColourId = colourId;
            foundStock.SizeId = sizeId;
            foundStock.ProductAmount = stockSize;

            _database.SaveChanges();
        }
        public void DeleteProductStocks(int stockId)
        {
            _database.ProductStocks.Remove(FindProductStock(stockId));
            _database.SaveChanges();
        }

        public async Task<List<ProductShowLightWeight>> SearchAsync(
            List<IFilter<Product>> filters,
            IOrdering<Product> paginator)
        {
            return await PerformSearchAsync(filters, paginator, 8);
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
                else
                {
                    throw new UserInteractionException("Для створення нового товару потрібно додати хоча б одне зображення.");
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
                if (vm.ProductImages != null)
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
