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
        private readonly IProductImagesManager _images;
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
            IProductImagesManager images,
			IProductsManager products,
			ILogger<ProductsController> logger)
		{
            _images = images;
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

        public IActionResult Index()
        {
            return View();
        }
    }
}