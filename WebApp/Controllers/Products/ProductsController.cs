using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApp.Database.Entities;
using WebApp.Helpers;
using WebApp.Helpers.Products.Filtering;
using WebApp.Helpers.Products.Filtering.Filters;
using WebApp.Helpers.Products.Filtering.OrderTypes;
using WebApp.Helpers.Products.Filtering.SortTypes;
using WebApp.ViewModels.Product;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Services.Interfaces.Grouping;
using WebApp.Services.Interfaces.Products;

namespace WebApp.Controllers.Products
{
    [Route("/products")]
	public class ProductsController : Controller
	{
		private readonly ProductFiltersFactory _filtersFactory;
		private readonly ProductOrderFactory _ordersFactory;

		private readonly IBrandsManager _brands;
		private readonly ICategoriesManager _categories;
		private readonly IProductImagesManager _images;
		private readonly IProductsManager _products;
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
			string? selectedOrderType)
		{
			ProductsSearchInitializator search = new ProductsSearchInitializator()
			{
				Query = query ?? string.Empty,
				Categories = _categories.GetSelectListWithSelectedId(selectedCategory ?? 0),
				Brands = _brands.GetSelectListWithSelectedId(selectedBrand ?? 0),
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

		private IActionResult PerformAction(
			Func<IActionResult> action, 
			Action<UserInteractionException>? onUserError, 
			Func<IActionResult> onFail)
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
			IBrandsManager brands,
			ICategoriesManager categories,
			IProductImagesManager images,
			IProductsManager products,
			ILogger<ProductsController> logger)
		{
			_brands = brands;
			_categories = categories;
			_images = images;
			_products = products;
			_logger = logger;
			_jsonSettings = new JsonSerializerSettings()
			{
				ContractResolver = new DefaultContractResolver()
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				}
			};

			_filtersFactory = new ProductFiltersFactory(
				new Dictionary<string, Func<string, IFilter<Product>>>()
				{
					{"brand", BelongsToBrand.CreateInstance},
					{"category", BelongsToCategory.CreateInstance},
					{"maxprice", MaxPrice.CreateInstance},
					{"minprice", MinPrice.CreateInstance},
					{"namecontains", NameContains.CreateInstance},
					{"minrating", MinRating.CreateInstance}
				}
			);

			_ordersFactory = new ProductOrderFactory(
				new Dictionary<string, Func<int, string, bool, IOrdering<Product>>>()
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
		public IActionResult Search(
			[FromQuery(Name = "maxid")] int maxId)
		{
			return PerformAction(
				() =>
				{
					Dictionary<string, string> searchParameters =
						Request.Query
							.Where(e => e.Key != "maxid")
							.ToDictionary(e => e.Key, e => e.Value.ToString());

					List<IFilter<Product>> filters = 
						_filtersFactory.ParseFilters(searchParameters);

					IOrdering<Product> paginator =
						_ordersFactory.CreateOrdering(maxId, searchParameters);

					//System.Text.Json does not support DateOnly objects for some reason. Using Newtonsoft.Json instead
					return Content(
						JsonConvert.SerializeObject(
							_products.Search(filters, paginator),
							_jsonSettings
						),
						"application/json"
					);
				},
				null,
				() => BadRequest()
			);
		}

		[HttpGet("product/{productId}")]
		public IActionResult Show(
			[FromRoute(Name = "productId")] int productId)
		{
			return PerformAction(
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
			return PerformAction(
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
			[FromQuery(Name = "order")] string? orderType)
		{
			return View(
				"ShowProductsList",
				GetSearchInitialization(query, categoryId, brandId, sortType, orderType)
			);
		}
	}
}