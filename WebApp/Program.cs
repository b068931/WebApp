using WebApp.Database;
using Microsoft.EntityFrameworkCore;
using WebApp.Services.Implementation.Grouping;
using WebApp.Services.Implementation.Products;
using WebApp.Database.Entities.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Unable to find database connection string.")));

builder.Services
	.AddIdentityCore<ApplicationUser>(options =>
	{
		options.SignIn.RequireConfirmedAccount = true;
		options.Password.RequiredLength = 10;
	})
	.AddRoles<ApplicationRole>()
	.AddEntityFrameworkStores<DatabaseContext>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
		options =>
		{
			options.LoginPath = new PathString("/auth/login");
			options.LogoutPath = new PathString("/auth/logout");
			options.AccessDeniedPath = new PathString("/auth/noentry");

			options.ExpireTimeSpan = TimeSpan.FromDays(5);
			options.SlidingExpiration = true;

			options.ReturnUrlParameter = "return";
		});

builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();
});

builder.Services.AddScoped<BrandsManager>();
builder.Services.AddScoped<CategoriesManager>();
builder.Services.AddScoped<ProductImagesManager>();
builder.Services.AddScoped<ProductsManager>();
builder.Services.AddScoped<ColoursManager>();
builder.Services.AddScoped<SizesManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");

	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); //Almost all static files are available anonymously

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
