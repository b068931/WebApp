using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Auth;
using WebApp.ProjectConfiguration.Initializers;
using WebApp.ProjectConfiguration.Options;
using WebApp.ProjectConfiguration.Options.Auth;
using WebApp.Services.Background;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Maintenance;
using WebApp.Services.Database.Products;
using WebApp.Utilities.CustomRequirements.SameAuthor;
using WebApp.Utilities.Exceptions;
using WebApp.Utilities.Other;

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
	options.AddPolicy("CriticalSiteContentPolicy", policy =>
	{
		policy
			.RequireAuthenticatedUser()
			.RequireRole("admin");
	});

	options.AddPolicy("PublicContentPolicy", policy =>
	{
		policy
			.RequireAuthenticatedUser()
			.RequireRole("admin", "user");
	});

	options.AddPolicy("MyContentPolicy", policy =>
	{
		policy
			.RequireAuthenticatedUser()
			.RequireRole("admin", "user")
			.AddRequirements(
				new SameAuthorRequirement()
			);
	});

	options.FallbackPolicy = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();
});

//Add app settings.
builder.Services.Configure<UserInteractionOptions>(
	builder.Configuration.GetSection(UserInteractionOptions.FieldName)
);

//Add custom services.
builder.Services
	.AddScoped<BrandsManager>()
	.AddScoped<CategoriesManager>()
	.AddScoped<ProductImagesManager>()
	.AddScoped<ProductsManager>()
	.AddScoped<ColoursManager>()
	.AddScoped<SizesManager>()
	.AddScoped<ProductStocksManager>()
	.AddScoped<UserInteractionManager>()
	.AddScoped(typeof(Performer<>))
	.AddHostedService<UserInteractionInformationCleanUpService>();

//Add custom requirements.
builder.Services
	.AddScoped<IAuthorizationHandler, SameAuthorAuthorizationHandler>();

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
		AuthConfiguration authConfig = new AuthConfiguration();
		app.Configuration.GetSection(AuthConfiguration.FieldName).Bind(authConfig);

		DatabaseInitializer initializer = new DatabaseInitializer(
			scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
			scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>(),
			app.Configuration
		);

		await initializer.InitializeAuthAsync(authConfig);
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
