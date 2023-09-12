using Newtonsoft.Json;

namespace WebApp.Database.Models
{
	public class ProductPreview
	{
		public int Id { get; set; }

		[JsonIgnore]
		public string MainImage { get; set; } = default!;
		[JsonIgnore]
		public string Name { get; set; } = default!;

		public decimal Price { get; set; }
		public int Discount { get; set; }
		public decimal TruePrice { get; set; }

		public int TrueRating { get; set; }
		public int ViewsCount { get; set; }

		public DateOnly Date { get; set; }

		[JsonIgnore]
		public List<ColourModel> AvailableColours { get; set; } = default!;

		[JsonIgnore]
		public List<SizeModel> AvailableSizes { get; set; } = default!;
	}
}
