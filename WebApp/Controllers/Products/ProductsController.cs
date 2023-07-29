using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using WebApp.Database.Entities;
using WebApp.Helpers;
using WebApp.Services.Interfaces;
using WebApp.ViewModels.Product;

namespace WebApp.Controllers.Products
{
    [Route("/products")]
    public class ProductsController : Controller
    {
        private readonly IProductsManager _products;
        private readonly ILogger<ProductsController> _logger;

        private IActionResult GetProductCreateView()
        {
            return View("ProductCreationForm", _products.GetProductCreateVM());
		}
        private IActionResult GetProductUpdateView(int productId)
        {
            return View("ProductUpdateForm", _products.GetProductUpdateVM(productId));
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
			IProductsManager products,
			ILogger<ProductsController> logger)
		{
            _products = products;
			_logger = logger;
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

        public IActionResult Index()
        {
            return View();
        }
    }
}
