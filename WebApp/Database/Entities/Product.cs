namespace WebApp.Database.Entities
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; } = default!;

		public int CategoryId { get; set; }
		public Category Category { get; set; } = default!;
	}
}
