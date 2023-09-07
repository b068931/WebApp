using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Auth;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Products;
using WebApp.Utilities.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DatabaseContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Unable to find database connection string.")));

builder.Services
	.AddIdentity<ApplicationUser, ApplicationRole>(options =>
	{
		options.SignIn.RequireConfirmedAccount = true;

		options.User.RequireUniqueEmail = true;
		options.User.AllowedUserNameCharacters = "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM_ ";

		options.Password.RequiredLength = 20; //The only strict requirement is the length of the password. This is to encourage the use of 'passPHRASES' instead of 'passWORDS'
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireDigit = false;
		options.Password.RequireLowercase = false;
		options.Password.RequireUppercase = false;
	})
	.AddEntityFrameworkStores<DatabaseContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = new PathString("/auth/login");
	options.LogoutPath = new PathString("/auth/logout");
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

using (var scope = app.Services.CreateScope())
{
	try
	{
		DatabaseInitializer initializer = new DatabaseInitializer(
			scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
			scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>(),
			app.Configuration
		);

		await initializer.InitializeAuthAsync();
	}
	catch (Exception exc)
	{
		throw new ServerInitializationException("Unable to initialize the database. This is required to create an admin account with a few initial roles.", exc);
	}
}

app.UseHttpsRedirection();
app.UseStaticFiles(); //Almost all static files are available anonymously. For other static files check AuthorizedStaticFilesController

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
