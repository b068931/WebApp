using WebApp.Database;
using Microsoft.EntityFrameworkCore;
using WebApp.Services.Interfaces;
using WebApp.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Unable to find database connection string.")));

builder.Services.AddScoped<IBrandsManager, BrandsDatabaseManager>();
builder.Services.AddScoped<ICategoriesManager, CategoriesDatabaseManager>();
builder.Services.AddScoped<IProductImagesManager, ProductImagesDatabaseManager>();
builder.Services.AddScoped<IProductsManager, ProductsDatabaseManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
