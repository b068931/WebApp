using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Product
{
    public class ProductCreate
    {
        [Required(ErrorMessage = "Будь ласка, вкажіть ім'я товару.")]
        [StringLength(maximumLength: 100, MinimumLength = 5, ErrorMessage = "Довжина імені товару має бути 5-100 символів.")]
        public string Name { get; set; } = default!;

        [StringLength(maximumLength: 6000, MinimumLength = 0, ErrorMessage = "Максимальна довжина опису товару - 6000 символів.")]
        public string Description { get; set; } = default!;

        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Знижка не може бути більшою за 100% (Можливо, ви хочете віддати ваш товар безкоштовно?)")]
        public int Discount { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        public List<IFormFile>? ProductImages { get; set; }

        [ValidateNever]
        public List<SelectListItem> AvailableCategories { get; set; } = default!;

        [ValidateNever]
        public List<SelectListItem> AvailableBrands { get; set; } = default!;
    }
}
