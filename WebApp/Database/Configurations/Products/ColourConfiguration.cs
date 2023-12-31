﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Database.Entities.Products;

namespace WebApp.Database.Configurations.Products
{
	public class ColourConfiguration : IEntityTypeConfiguration<Colour>
	{
		public void Configure(EntityTypeBuilder<Colour> builder)
		{
			builder
				.ToTable("ProductColours")
				.HasKey(e => e.Id);

			builder
				.HasIndex(e => new { e.Name, e.HexCode })
				.IsUnique(true);

			builder
				.Property(e => e.Name)
				.HasMaxLength(50);
		}
	}
}
