using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApp.Helpers;
using WebApp.Helpers.Products.Filtering;
using WebApp.Helpers.Products.Filtering.Filters;
using WebApp.Helpers.Products.Filtering.OrderTypes;
using WebApp.Helpers.Products.Filtering.SortTypes;
using WebApp.ViewModels.Product;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using WebApp.Helpers.Filtering;
using WebApp.Helpers.Exceptions;
using WebApp.Database.Entities.Products;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Products;

namespace WebApp.Controllers.Products
{
    [Route("/products")]
	public class ProductsController : Controller
	{
		private readonly ProductFiltersFactory _filtersFactory;
		private readonly ProductOrderFactory _ordersFactory;

		private readonly ColoursManager _colours;
		private readonly SizesManager _sizes;
		private readonly BrandsManager _brands;
		private readonly CategoriesManager _categories;
		private readonly ProductImagesManager _images;
		private readonly ProductsManager _products;
		private readonly ILogger<ProductsController> _logger;
		private readonly JsonSerializerSettings _jsonSettings;

		private IActionResult GetProductCreateView()
		{
			return View("ProductCreationForm", _products.GetProductCreateVM());
		}
		private IActionResult GetProductUpdateView(int productId)
		{
			return View("ProductUpdateForm", _products.GetProductUpdateVM(productId));
		}
		private ProductsSearchInitializator GetSearchInitialization(
			string? query,
			int? selectedCategory,
			int? selectedBrand,
			string? selectedSortType,
			string? selectedOrderType,
			string? minDate,
			int? minRatingsCount)
		{
			ProductsSearchInitializator search = new ProductsSearchInitializator()
			{
				Query = query ?? string.Empty,
				MinDate = minDate ?? string.Empty,
				MinRatingsCount = minRatingsCount ?? 0,
				Categories = _categories.GetSelectListWithSelectedId(selectedCategory ?? 0),
				Brands = _brands.GetSelectListWithSelectedId(selectedBrand ?? 0),
				Colours = _colours.GetSelectList(),
				Sizes = _sizes.GetSelectList(),
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

			if(selectedSortType != null)
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
			if(selectedOrderType != null)
			{
				foreach(var orderType in search.Directions)
				{
					if(orderType.Value == selectedOrderType)
					{
						orderType.Selected = true;
						break;
					}
				}
			}

			return search;
		}

		private ResultT PerformAction<ResultT>(
			Func<ResultT> action, 
			Action<UserInteractionException>? onUserError, 
			Func<ResultT> onFail)
		{
			try
			{
				return action();
			}
			catch (UserInteractionException ex)
			{
				if(onUserError != null)
				{
					onUserError(ex);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ProductsController error.");
			}

			return onFail();
		}
		
		public ProductsController(
			BrandsManager brands,
			CategoriesManager categories,
			ProductImagesManager images,
			ProductsManager products,
			ColoursManager colours,
			SizesManager sizes,
			ILogger<ProductsController> logger)
		{
			_brands = brands;
			_categories = categories;
			_images = images;
			_products = products;
			_colours = colours;
			_sizes = sizes;
			_logger = logger;
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
		public Task<IActionResult> Search(
			[FromQuery(Name = "maxid")] int maxId,
			[FromQuery(Name = "includesearchresult")] bool isSearchResultIncluded = true)
		{
			return PerformAction<Task<IActionResult>>(
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

					List<Database.Models.ProductShowLightWeight> searchResult = 
						await _products.SearchAsync(filters, paginator);

					string html = await ControllerExtenstions.RenderViewAsync(
						this,
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
				null,
				async () => await Task.Run(() => BadRequest())
			);
		}

		[HttpGet("product/{productId}")]
		public IActionResult Show(
			[FromRoute(Name = "productId")] int productId)
		{
			return PerformAction<IActionResult>(
				() => View("ShowProduct", _products.GetProductShowVM(productId)), 
				null,
				() => Redirect("/")
			);
		}

		[HttpGet("action/create")]
		public IActionResult Create()
		{
			return GetProductCreateView();
		}

		[HttpPost("action/create")]
		[ValidateAntiForgeryToken]
		public IActionResult Create(ProductCreate vm)
		{
			if (!ModelState.IsValid)
			{
				return GetProductCreateView();
			}

			return PerformAction(
				() =>
				{
					Product createdProduct = _products.CreateProduct(vm);
					return Redirect("/products/product/" + createdProduct.Id);
				},
				(ex) => ModelState.AddModelError(string.Empty, ex.Message),
				() => GetProductCreateView()
			);
		}

		[HttpGet("action/update")]
		public IActionResult Update(
			[FromQuery(Name = "id")] int productId)
		{
			return PerformAction(
				() => GetProductUpdateView(productId),
				(ex) => ModelState.AddModelError(string.Empty, ex.Message), 
				() => GetProductCreateView()
			);
		}

		[HttpPost("action/update")]
		[ValidateAntiForgeryToken]
		public IActionResult Update(ProductUpdate vm)
		{
			if (!ModelState.IsValid)
			{
				return PerformAction(
					() => GetProductUpdateView(vm.Id),
					(ex) => ModelState.AddModelError(string.Empty, ex.Message),
					() => GetProductCreateView()
				);
			}

			return PerformAction(
				() =>
				{
					_products.UpdateProduct(vm);
					return Redirect("/products/product/" + vm.Id);
				},
				(ex) => ModelState.AddModelError(string.Empty, ex.Message),
				() => GetProductCreateView()
			);
		}

		[HttpGet("images/action/update")]
		public IActionResult UpdateImages(
			[FromQuery(Name = "id")] int productId)
		{
			return View("ProductImagesUpdateForm", (_images.GetProductImages(productId), productId));
		}

		[HttpPost("images/action/update")]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateImages(
			[FromForm(Name = "newMainImageId")] int mainImageId,
			[FromForm(Name = "productId")] int productId,
			[FromForm(Name = "deleteImages")] List<int> imagesToDelete)
		{
			return PerformAction<IActionResult>(
				() =>
				{
					Product associatedProduct = _products.FindProduct(productId);
					if (mainImageId != 0)
						_products.ChangeMainImage(productId, mainImageId);

					if (imagesToDelete.Contains(_products.GetProductMainImage(productId)))
						throw new UserInteractionException("You can not delete a main image of your product.");
					else
						_images.DeleteImages(imagesToDelete);

					return Redirect("/products/product/" + productId);
				},
				(ex) => ModelState.AddModelError("", ex.Message),
				() => View("ProductImagesUpdateForm", (_images.GetProductImages(productId), productId))
			);
		}

		[HttpPost("action/delete")]
		[ValidateAntiForgeryToken]
		public IActionResult Delete(
			[FromForm(Name = "id")] int idToDelete)
		{
			return PerformAction(
				() =>
				{
					_products.DeleteProduct(idToDelete);
					return Redirect("/");
				},
				null,
				() => Redirect("/")
			);
		}

		public IActionResult Index(
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
				GetSearchInitialization(
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