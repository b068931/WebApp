using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Abstract;

namespace WebApp.Database.Configurations.Abstract
{
	public abstract class UserProductMappingWithDateConfiguration<T> : IEntityTypeConfiguration<T> where T : UserProductMappingWithDate
	{
		public virtual void Configure(EntityTypeBuilder<T> builder)
		{
			builder.HasKey(e => new { e.UserId, e.ProductId });

			builder
				.HasOne(e => e.Product)
				.WithMany()
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			builder
				.HasOne(e => e.AssociatedUser)
				.WithMany()
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			builder
				.Property(e => e.InteractionDate)
				.HasDefaultValueSql("getdate()");
		}
	}
}
