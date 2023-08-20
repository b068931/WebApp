using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Product
{
	public class ProductShow
	{
        public int Id { get; set; }

        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;

        public decimal Price { get; set; }
        public int Discount { get; set; }

        public int ViewsCount { get; set; }
        public int Rating { get; set; }
        public int ReviewsCount { get; set; }

        public (string Name, int ImageId)? BrandInfo { get; set; }

        public int MainImageId { get; set; }
        public List<int> ProductImagesIds { get; set; } = default!;
    }
}
