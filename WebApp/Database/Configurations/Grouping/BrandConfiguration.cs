﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Grouping;

namespace WebApp.Database.Configurations.Grouping
{
	public class BrandConfiguration : IEntityTypeConfiguration<Brand>
	{
		public void Configure(EntityTypeBuilder<Brand> builder)
		{
			builder
				.ToTable("Brands")
				.HasKey(c => c.Id);

			builder
				.HasOne(e => e.Image)
				.WithOne(e => e.Brand)
				.HasForeignKey<Brand>(e => e.ImageId)
				.OnDelete(DeleteBehavior.ClientSetNull);

			builder
				.Property(e => e.Name)
				.HasMaxLength(100);
		}
	}
}
