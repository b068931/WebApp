﻿using WebApp.Database.Models;
using WebApp.ViewModels.Categories;

namespace WebApp.ViewModels
{
	public class AboutUsVM
	{
		public List<CategoryVM> PopularCategories { get; set; } = default!;
		public List<Brand> Brands { get; set; } = default!;
	}
}
