using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers.Exceptions;
using WebApp.Services.Interfaces;
using WebApp.ViewModels.Product;

namespace WebApp.Controllers.Products
{
    [Route("/products")]
    public class ProductsController : Controller
    {
        private readonly IProductsManager _products;
        private readonly ICategoriesManager _categories;
        private readonly IBrandsManager _brands;
        private readonly ILogger<ProductsController> _logger;

		private ProductCreate GetProductCreationVM()
        {
            return new ProductCreate()
            {
                AvailableCategories = _categories.GetSelectList(),
                AvailableBrands = _brands.GetSelectList()
            };
        }
        private IActionResult PerformAction(Action action)
        {
            try
            {
                action();
            }
            catch (ProductInteractionException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProductsController error.");
            }

            return View("ProductCreationForm", GetProductCreationVM());
        }
        
		public ProductsController(
			IProductsManager products,
			ICategoriesManager categories,
			IBrandsManager brands,
			ILogger<ProductsController> logger)
		{
            _products = products;
			_categories = categories;
			_brands = brands;
			_logger = logger;
		}

        [HttpGet("product/{productId}")]
        public IActionResult Show(
            [FromRoute(Name = "productId")] int productId)
        {
            return View("ShowProduct", )
        }

		[HttpGet("action/create")]
        public IActionResult Create()
        {
            return View("ProductCreationForm", GetProductCreationVM());
        }

        [HttpPost("action/create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCreate vm)
        {
            if (!ModelState.IsValid)
            {
                return View("ProductCreationForm", GetProductCreationVM());
            }

            return PerformAction(() => _products.CreateProduct(vm));
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
