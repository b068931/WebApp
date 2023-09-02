using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Entities.Grouping
{
    [EntityTypeConfiguration(typeof(CategoryConfiguration))]
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        public bool IsLast { get; set; }
        public bool IsPopular { get; set; }

        public int? ParentId { get; set; }
        public Category? Parent { get; set; }

        public List<Category> Children { get; set; } = default!;
        public List<Product> Products { get; set; } = default!;
    }
}
