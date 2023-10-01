using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApp.Controllers.Abstract;
using WebApp.Database.Entities.Products;
using WebApp.Database.Models;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Products;
using WebApp.Utilities.CustomRequirements.SameAuthor;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Filtering;
using WebApp.Utilities.Filtering.Products;
using WebApp.Utilities.Filtering.Products.Filters;
using WebApp.Utilities.Filtering.Products.OrderTypes;
using WebApp.Utilities.Other;
using WebApp.ViewModels.Product;

namespace WebApp.Controllers.Products
{
	[Route("/products")]
	[Authorize(Policy = "PublicContentPolicy")]
	public class ProductsController : ExtendedController
	{
		private const int ProductsPageSize = 8;

		private readonly ProductFiltersFactory _filtersFactory;
		private readonly ProductOrderFactory _ordersFactory;

		private readonly ColoursManager _colours;
		private readonly SizesManager _sizes;

		private readonly BrandsManager _brands;
		private readonly CategoriesManager _categories;

		private readonly ProductImagesManager _images;
		private readonly ProductsManager _products;

		private readonly Performer<ProductsController> _performer;
		private readonly JsonSerializerSettings _jsonSettings;

		private async Task<Author> GetTrueAuthorAsync(int productId)
		{
			return new Author()
			{
				Id = await _products.GetProductOwnerAsync(productId)
			};
		}

		private async Task<IActionResult> GetProductCreateViewAsync()
		{
			return View("ProductCreationForm", await _products.GetProductCreateVMAsync());
		}
		private async Task<IActionResult> GetProductUpdateViewAsync(int productId)
		{
			return View("ProductUpdateForm", await _products.GetProductUpdateVMAsync(productId));
		}
		private async Task<ProductsSearchInitializator> GetSearchInitializationAsync(
			string? query,
			int? selectedCategory,
			int? selectedBrand,
			string? selectedSortType,
			string? selectedOrderType,
			string? minDate,
			int? minRatingsCount)
		{
			ProductsSearchInitializator search = new()
			{
				Query = query ?? string.Empty,
				MinDate = minDate ?? string.Empty,
				MinRatingsCount = minRatingsCount ?? 0,
				Categories = await _categories.GetSelectListWithSelectedIdAsync(selectedCategory ?? 0),
				Brands = await _brands.GetSelectListWithSelectedIdAsync(selectedBrand ?? 0),
				Colours = await _colours.GetSelectListAsync(),
				Sizes = await _sizes.GetSelectListAsync(),
				SortTypes = new List<SelectListItem>()
				{
					new SelectListItem()
					{
						Text = "Дата створення",
						Value = "date"
					},
					new SelectListItem()
					{
						Text = "Ціна (зі знижкою)",
						Value = "price"
					},
					new SelectListItem()
					{
						Text = "Знижки",
						Value = "discount"
					},
					new SelectListItem()
					{
						Text = "Кількість переглядів",
						Value = "views"
					},
					new SelectListItem()
					{
						Text = "Оцінки",
						Value = "stars"
					}
				},
				Directions = new List<SelectListItem>()
				{
					new SelectListItem()
					{
						Text = "Зростання",
						Value = "regular"
					},
					new SelectListItem()
					{
						Text = "Спадання",
						Value = "reversed"
					}
				}
			};

			if (selectedSortType != null)
			{
				foreach (var sortType in search.SortTypes)
				{
					if (sortType.Value == selectedSortType)
					{
						sortType.Selected = true;
						break;
					}
				}
			}
			if (selectedOrderType != null)
			{
				foreach (var orderType in search.Directions)
				{
					if (orderType.Value == selectedOrderType)
					{
						orderType.Selected = true;
						break;
					}
				}
			}

			return search;
		}

		public ProductsController(
			BrandsManager brands,
			CategoriesManager categories,
			ProductImagesManager images,
			ProductsManager products,
			ColoursManager colours,
			SizesManager sizes,
			Performer<ProductsController> performer)
		{
			_brands = brands;
			_categories = categories;
			_images = images;
			_products = products;
			_colours = colours;
			_sizes = sizes;
			_performer = performer;
			_jsonSettings = new JsonSerializerSettings()
			{
				ContractResolver = new DefaultContractResolver()
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				}
			};

			_filtersFactory = new ProductFiltersFactory(
				new Dictionary<string, Func<StringValues, IFilter<Product>>>()
				{
					{"brand[]", BelongsToBrand.CreateInstance},
					{"category[]", BelongsToCategory.CreateInstance},
					{"colour[]", PresentColour.CreateInstance},
					{"psize[]", PresentSize.CreateInstance},
					{"maxprice", MaxPrice.CreateInstance},
					{"minprice", MinPrice.CreateInstance},
					{"namecontains", NameContains.CreateInstance},
					{"minrating", MinRating.CreateInstance},
					{"minratingscount", MinReviewsCount.CreateInstance},
					{"mincreated", MinDate.CreateInstance},
					{"maxcreated", MaxDate.CreateInstance}
				}
			);

			_ordersFactory = new ProductOrderFactory(
				new Dictionary<string, Func<int, StringValues, bool, IOrdering<Product>>>()
				{
					{"sortdate", DateOrder.CreateInstance},
					{"sortviews", ViewsOrder.CreateInstance},
					{"sortstars", RatingsOrder.CreateInstance},
					{"sortprice", PriceOrder.CreateInstance},
					{"sortdiscount", DiscountOrder.CreateInstance}
				}
			);
		}

		[HttpGet("search")]
		[AllowAnonymous]
		public Task<IActionResult> Search(
			[FromQuery(Name = "maxid")] int maxId,
			[FromQuery(Name = "includesearchresult")] bool isSearchResultIncluded = true)
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					Dictionary<string, StringValues> searchParameters =
						Request.Query
							.Where(e => e.Key != "maxid")
							.ToDictionary(e => e.Key, e => e.Value);

					List<IFilter<Product>> filters =
						_filtersFactory.ParseFilters(searchParameters);

					IOrdering<Product> paginator =
						_ordersFactory.CreateOrdering(maxId, searchParameters);

					List<ProductPreview> searchResult =
						await _products.SearchAsync(filters, paginator, ProductsPageSize);

					string html = await RenderViewAsync(
						"_ProductSearchPreview",
						searchResult,
						true
					);

					//System.Text.Json does not support DateOnly objects for some reason. Using Newtonsoft.Json instead
					return Content(
						JsonConvert.SerializeObject(
							isSearchResultIncluded
								? new { searchResult, html }
								: new { html },
							_jsonSettings
						),
						"application/json"
					);
				},
				(message) => BadRequest(message)
			);
		}

		[HttpGet("product/{productId}")]
		[AllowAnonymous]
		public Task<IActionResult> Show(
			[FromRoute(Name = "productId")] int productId)
		{
			return _performer.PerformInertActionAsync(
				async () => View(
					"ShowProduct",
					await _products.GetProductShowVMAsync(GetUserId(), productId)
				),
				() => Redirect("/")
			);
		}

		[HttpPost("product/{productId}/rate")]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> RateProduct(
			[FromRoute(Name = "productId")] int productId,
			[FromForm(Name = "stars")] int stars)
		{
			return _performer.PerformActionMessageAsync(
				async () =>
				{
					if (GetUserId() == 0)
						throw new UserInteractionException("Зареєструйтеся для того, щоб залишити оцінку.");

					await _products.RateProductAsync(GetUserId(), productId, stars);
					return Ok();
				},
				(message) => BadRequest(message)
			);
		}

		[HttpGet("action/create")]
		public Task<IActionResult> Create()
		{
			return GetProductCreateViewAsync();
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Create(ProductCreate vm)
		{
			if (!ModelState.IsValid)
			{
				return GetProductCreateViewAsync();
			}

			return _performer.PerformActionAsync(
				async () =>
				{
					Product createdProduct = await _products.CreateProductAsync(GetUserId(), vm);
					return Redirect("/products/product/" + createdProduct.Id);
				},
				(ex) => ModelState.AddModelError(string.Empty, ex.Message),
				() => GetProductCreateViewAsync()
			);
		}

		[HttpGet("action/update")]
		public Task<IActionResult> Update(
			[FromQuery(Name = "id")] int productId)
		{
			return _performer.PerformActionAsync(
				() => _performer.EnforceSameAuthorWrapperAsync(
					() => GetTrueAuthorAsync(productId),
					User,
					() => GetProductUpdateViewAsync(productId)
				),
				(ex) => ModelState.AddModelError(string.Empty, ex.Message),
				() => GetProductCreateViewAsync()
			);
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Update(ProductUpdate vm)
		{
			return _performer.PerformActionAsync(
				() => _performer.EnforceSameAuthorWrapperAsync(
					() => GetTrueAuthorAsync(vm.Id),
					User,
					async () =>
					{
						if (!ModelState.IsValid)
						{
							return await GetProductUpdateViewAsync(vm.Id);
						}

						await _products.UpdateProductAsync(vm);
						return Redirect("/products/product/" + vm.Id);
					}
				),
				(ex) => ModelState.AddModelError(string.Empty, ex.Message),
				() => GetProductUpdateViewAsync(vm.Id)
			);
		}

		[HttpGet("images/action/update")]
		public Task<IActionResult> UpdateImages(
			[FromQuery(Name = "id")] int productId)
		{
			return _performer.PerformInertActionAsync(
				() => _performer.EnforceSameAuthorWrapperAsync(
					() => GetTrueAuthorAsync(productId),
					User,
					async () => View(
						"ProductImagesUpdateForm",
						(await _images.GetProductImagesAsync(productId), productId)
					)
				),
				() => Redirect("/products/product/" + productId)
			);
		}

		[HttpPost("images/action/update")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> UpdateImages(
			[FromForm(Name = "newMainImageId")] int mainImageId,
			[FromForm(Name = "productId")] int productId,
			[FromForm(Name = "deleteImages")] List<int> imagesToDelete)
		{
			return _performer.PerformActionAsync(
				() => _performer.EnforceSameAuthorWrapperAsync(
					() => GetTrueAuthorAsync(productId),
					User,
					async () =>
					{
						if (mainImageId != 0)
							await _products.ChangeMainImageAsync(productId, mainImageId);

						if (imagesToDelete.Contains(await _products.GetProductMainImageAsync(productId)))
							throw new UserInteractionException("Ви не можете видалити головне зображення вашого продукту.");
						else
							await _images.DeleteImagesAsync(productId, imagesToDelete);

						return Redirect("/products/product/" + productId);
					}
				),
				(ex) => ModelState.AddModelError("", ex.Message),
				async () => View(
					"ProductImagesUpdateForm",
					(await _images.GetProductImagesAsync(productId), productId)
				)
			);
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public Task<IActionResult> Delete(
			[FromForm(Name = "id")] int idToDelete)
		{
			return _performer.PerformInertActionAsync(
				() => _performer.EnforceSameAuthorWrapperAsync(
					() => GetTrueAuthorAsync(idToDelete),
					User,
					async () =>
					{
						List<int> images = await
							_images.GetProductImagesAsync(idToDelete)
								.ContinueWith(next =>
									next.Result
										.Select(e => e.Id)
										.ToList()
								);

						await _images.DeleteImagesAsync(idToDelete, images);
						await _products.DeleteProductAsync(idToDelete);

						return Redirect("/");
					}
				),
				() => Redirect("/")
			);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index(
			[FromQuery(Name = "query")] string? query,
			[FromQuery(Name = "brand")] int? brandId,
			[FromQuery(Name = "category")] int? categoryId,
			[FromQuery(Name = "sort")] string? sortType,
			[FromQuery(Name = "order")] string? orderType,
			[FromQuery(Name = "mindate")] string? minDate,
			[FromQuery(Name = "minratings")] int? minRatingsCount)
		{
			return View(
				"ShowProductsList",
				await GetSearchInitializationAsync(
					query,
					categoryId,
					brandId,
					sortType,
					orderType,
					minDate,
					minRatingsCount
				)
			);
		}
	}
}