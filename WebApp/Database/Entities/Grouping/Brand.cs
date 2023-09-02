using Microsoft.EntityFrameworkCore;
using WebApp.Database.Configurations;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Entities.Grouping
{
    [EntityTypeConfiguration(typeof(BrandConfiguration))]
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;

        public int? ImageId { get; set; }
        public BrandImage? Image { get; set; } = default!;

        public List<Product> Products { get; set; } = default!;
    }
}
