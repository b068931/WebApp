namespace WebApp.Database.Models
{
	public class BrandImageModel
	{
		public string ContentType { get; set; } = default!;
		public byte[] ImageData { get; set; } = default!;
	}
}
