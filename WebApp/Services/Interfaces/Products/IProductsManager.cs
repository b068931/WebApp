﻿using WebApp.Database.Entities;
using WebApp.Database.Models;
using WebApp.Helpers;
using WebApp.ViewModels.Product;

namespace WebApp.Services.Interfaces.Products
{
    public interface IProductsManager
    {
        Product FindProduct(int productId);
        int GetProductMainImage(int productId);

        List<ProductShowLightWeightJson> Search(
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
