namespace WebApp.Database.Models
{
	public class BrandModel
	{
		public int Id { get; set; }

		public string Name { get; set; } = default!;
		public int ImageId { get; set; }
	}
}
