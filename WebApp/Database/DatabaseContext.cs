using Microsoft.EntityFrameworkCore;
using WebApp.Database.Entities;

namespace WebApp.Database
{
	public class DatabaseContext : DbContext
	{
		public DatabaseContext(DbContextOptions options) 
			: base(options)
		{
			Database.EnsureCreated();
		}

		public DbSet<Category> Categories { get; set; }

		public DbSet<Product> Products { get; set; }
		public DbSet<ProductImage> ProductImages { get; set; }

		public DbSet<Brand> Brands { get; set; }
		public DbSet<BrandImage> BrandImages { get; set; }

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

			modelBuilder.Entity<Brand>()
				.ToTable("Brands")
				.HasKey(c => c.Id);

			modelBuilder.Entity<BrandImage>()
				.ToTable("BrandImages")
				.HasKey(c => c.Id);

			modelBuilder.Entity<Brand>()
				.HasOne(e => e.Image)
				.WithOne(e => e.Brand)
				.HasForeignKey<Brand>(e => e.ImageId)
				.OnDelete(DeleteBehavior.ClientSetNull);

			modelBuilder.Entity<ProductImage>()
				.ToTable("ProductImages")
				.HasKey(c => c.Id);

			modelBuilder.Entity<ProductImage>()
				.HasOne(e => e.Product)
				.WithMany(e => e.Images)
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Product>()
				.ToTable("Products")
				.HasKey(c => c.Id);

			modelBuilder.Entity<Product>()
				.HasOne(e => e.Brand)
				.WithMany(e => e.Products)
				.HasForeignKey(e => e.BrandId)
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<Product>()
				.HasOne(e => e.Category)
				.WithMany(e => e.Products)
				.HasForeignKey(e => e.CategoryId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Product>()
				.HasOne(e => e.MainImage)
				.WithMany()
				.HasForeignKey(e => e.MainImageId)
				.OnDelete(DeleteBehavior.ClientSetNull);
		}
	}
}
