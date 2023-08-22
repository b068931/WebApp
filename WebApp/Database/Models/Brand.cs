namespace WebApp.Database.Models
{
	public class Brand
	{
		public int Id { get; set; }

		public string Name { get; set; } = default!;
		public int ImageId { get; set; }
	}
}
