namespace WebApp.Database.Models.Images
{
	public class BrandImageModel
	{
		public string ContentType { get; set; } = default!;
		public byte[] ImageData { get; set; } = default!;
	}
}
