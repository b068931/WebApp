using Newtonsoft.Json;

namespace WebApp.Database.Models
{
	public class ProductShowLightWeight
	{
		public int Id { get; set; }

		[JsonIgnore]
		public int MainImageId { get; set; }
		[JsonIgnore]
		public string Name { get; set; } = default!;

		public decimal Price { get; set; }
		public int Discount { get; set; }
		public decimal TruePrice { get; set; }

		public int TrueRating { get; set; }
		public int ViewsCount { get; set; }

		public DateOnly Date { get; set; }

		[JsonIgnore]
		public List<Colour> AvailableColours { get; set; } = default!;

		[JsonIgnore]
		public List<Size> AvailableSizes { get; set; } = default!;
	}
}
