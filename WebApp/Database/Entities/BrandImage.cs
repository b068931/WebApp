namespace WebApp.Database.Entities
{
	public class BrandImage
	{
		public int Id { get; set; }

		public byte[] Data { get; set; } = default!;
		public string ContentType { get; set; } = default!;

		public Brand Brand { get; set; } = default!;
	}
}
