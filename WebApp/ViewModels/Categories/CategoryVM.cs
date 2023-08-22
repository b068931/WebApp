namespace WebApp.ViewModels.Categories
{
	public class CategoryVM
	{
		public int Id { get; set; }

		public bool IsPopular { get; set; }
		public bool IsLast { get; set; }

		public string Name { get; set; } = default!;
		public List<CategoryVM> SubCategories { get; set; } = default!;
	}
}
