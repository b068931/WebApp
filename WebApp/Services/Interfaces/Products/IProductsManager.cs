using WebApp.Database.Entities.Products;
using WebApp.Database.Models;
using WebApp.Helpers.Filtering;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Interfaces.Products
{
    public interface IProductsManager
    {
        Product FindProduct(int productId);
        int GetProductMainImage(int productId);

        List<Database.Models.ProductStock> GetProductStocks(int productId);
        void CreateProductStocks(int productId, int colourId, int sizeId, int stockSize);
        void UpdateProductStocks(int stockId, int colourId, int sizeId, int stockSize);
        void DeleteProductStocks(int stockId);

        Task<List<ProductShowLightWeight>> SearchAsync(
            List<IFilter<Product>> filters,
            IOrdering<Product> paginator
        );

        ProductShow GetProductShowVM(int productId);
        ProductUpdate GetProductUpdateVM(int productId);
        ProductCreate GetProductCreateVM();

        Product CreateProduct(ProductCreate vm);
        void UpdateProduct(ProductUpdate vm);
        void DeleteProduct(int productId);
        void ChangeMainImage(int productId, int newMainImageId);
    }
}
