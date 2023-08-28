using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels.Product
{
	public class ProductsSearchInitializator
	{
		public string Query { get; set; } = default!;

		public List<SelectListItem> Categories { get; set; } = default!;
		public List<SelectListItem> Brands { get; set; } = default!;

		public List<SelectListItem> SortTypes { get; set; } = default!;
		public List<SelectListItem> Directions { get; set; } = default!;

		public List<SelectListItem> Colours { get; set; } = default!;
		public List<SelectListItem> Sizes { get; set; } = default!;
	}
}
