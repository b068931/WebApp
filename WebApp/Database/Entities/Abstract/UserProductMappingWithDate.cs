using WebApp.Database.Entities.Auth;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Entities.Abstract
{
	public abstract class UserProductMappingWithDate
	{
		public int ProductId { get; set; }
		public Product Product { get; set; } = default!;

		public int UserId { get; set; }
		public ApplicationUser AssociatedUser { get; set; } = default!;

		public DateOnly InteractionDate { get; set; }
	}
}
