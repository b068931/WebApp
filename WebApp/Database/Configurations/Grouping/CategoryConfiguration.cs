using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Grouping;

namespace WebApp.Database.Configurations.Grouping
{
	public class CategoryConfiguration : IEntityTypeConfiguration<Category>
	{
		public void Configure(EntityTypeBuilder<Category> builder)
		{
			builder
				.ToTable("Categories")
				.HasKey(c => c.Id);

			builder
				.HasOne(e => e.Parent)
				.WithMany(e => e.Children)
				.HasForeignKey(e => e.ParentId)
				.OnDelete(DeleteBehavior.ClientSetNull);

			builder
				.Property(e => e.Name)
				.HasMaxLength(50);
		}
	}
}
