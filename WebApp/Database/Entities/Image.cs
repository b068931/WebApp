namespace WebApp.Database.Entities
{
	public class Image
	{
		public int Id { get; set; }
		public byte[] Data { get; set; } = default!;
		public string ContentType { get; set; } = default!;

		public int ProductId { get; set; }
		public Product Product { get; set; } = default!;
	}
}
