using Microsoft.EntityFrameworkCore;
using WebApp.Database.Entities;

namespace WebApp.Database
{
	public class DatabaseContext : DbContext
	{
		public DatabaseContext(DbContextOptions options) 
			: base(options)
		{
			this.Database.EnsureCreated();
		}

		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Category>()
				.ToTable("Categories")
				.HasKey(c => c.Id);

			modelBuilder.Entity<Category>()
				.HasOne(e => e.Parent)
				.WithMany(e => e.Children)
				.HasForeignKey(e => e.ParentId)
				.OnDelete(DeleteBehavior.ClientSetNull);
				
			modelBuilder.Entity<Product>()
				.ToTable("Products")
				.HasKey(c => c.Id);

			modelBuilder.Entity<Product>()
				.HasOne(e => e.Category)
				.WithMany(e => e.Products)
				.HasForeignKey(e => e.CategoryId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
