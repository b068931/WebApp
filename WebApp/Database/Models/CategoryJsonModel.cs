namespace WebApp.Database.Models
{
	public class CategoryJsonModel
	{
		public int Id { get; set; }
		public string Name { get; set; } = default!;

		public bool IsLast { get; set; }
		public bool IsPopular { get; set; }
	}
}
