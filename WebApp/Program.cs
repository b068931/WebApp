using WebApp.Database;
using Microsoft.EntityFrameworkCore;
using WebApp.Services.Interfaces.Grouping;
using WebApp.Services.Interfaces.Products;
using WebApp.Services.Implementation.Grouping;
using WebApp.Services.Implementation.Products;
using WebApp.Database.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Unable to find database connection string.")));

builder.Services
	.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<DatabaseContext>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
		options =>
		{
			options.LoginPath = new PathString("/auth/login");
			options.ReturnUrlParameter = "return";
		});

builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IBrandsManager, BrandsDatabaseManager>();
builder.Services.AddScoped<ICategoriesManager, CategoriesDatabaseManager>();
builder.Services.AddScoped<IProductImagesManager, ProductImagesDatabaseManager>();
builder.Services.AddScoped<IProductsManager, ProductsDatabaseManager>();
builder.Services.AddScoped<IColoursManager, ColoursDatabaseManager>();
builder.Services.AddScoped<ISizesManager, SizesDatabaseManager>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
