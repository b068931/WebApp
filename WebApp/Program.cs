using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Database;
using WebApp.Database.Entities.Auth;
using WebApp.ProjectConfiguration.Constants;
using WebApp.ProjectConfiguration.Initializers;
using WebApp.ProjectConfiguration.Options;
using WebApp.ProjectConfiguration.Options.Auth;
using WebApp.ProjectConfiguration.Options.Email;
using WebApp.Services.Actions;
using WebApp.Services.Background;
using WebApp.Services.Database.Grouping;
using WebApp.Services.Database.Maintenance;
using WebApp.Services.Database.Products;
using WebApp.Utilities.CustomIdentityComponents;
using WebApp.Utilities.CustomIdentityComponents.Requirements.SameAuthor;
using WebApp.Utilities.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DatabaseContext>(options =>
{
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("Database")
			?? throw new InvalidOperationException("Unable to find database connection string."),
		sqlServerOptions =>
		{
			sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
		}
	);
});

builder.Services
	.AddIdentity<ApplicationUser, ApplicationRole>(options =>
	{
		options.SignIn.RequireConfirmedAccount = true;

		options.User.RequireUniqueEmail = true;
		options.User.AllowedUserNameCharacters = string.Empty;

		options.Password.RequiredLength = 20; //The only strict requirement is the length of the password. This is to encourage the use of 'passPHRASES' instead of 'passWORDS'
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireDigit = false;
		options.Password.RequireLowercase = false;
		options.Password.RequireUppercase = false;
	})
	.AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
	.AddEntityFrameworkStores<DatabaseContext>()
	.AddDefaultTokenProviders()
	.AddTokenProvider<AccountDeletionEmailConfirmationTokenProvider>(CustomTokenProviders.DeleteUserAccount);

builder.Services
	.AddAuthentication()
	.AddGoogle(googleAuth =>
	{
		googleAuth.CorrelationCookie.Name = "GoogleCorrelationCookie";
		googleAuth.CallbackPath = new PathString("/auth/login/external/google");

		googleAuth.ReturnUrlParameter = "return";
		googleAuth.AccessDeniedPath = new PathString("/auth/noentry");

		googleAuth.ClientId = builder.Configuration["GoogleOAuth:ClientID"]
			?? throw new ArgumentNullException("GoogleClientID", "Unable to find Google OAuth client id.");

		googleAuth.ClientSecret = builder.Configuration["GoogleOAuth:ClientSecret"]
			?? throw new ArgumentNullException("GoogleClientSecret", "Unable to find Google OAuth client secret.");
	})
	.AddMicrosoftAccount(microsoftAuth =>
	{
		microsoftAuth.CorrelationCookie.Name = "MicrosoftCorrelationCookie";
		microsoftAuth.CallbackPath = new PathString("/auth/login/external/microsoft");

		microsoftAuth.ReturnUrlParameter = "return";
		microsoftAuth.AccessDeniedPath = new PathString("/auth/noentry");

		microsoftAuth.ClientId = builder.Configuration["MicrosoftOAuth:ClientID"]
			?? throw new ArgumentNullException("MicrosoftClientID", "Unable to find Microsoft OAuth client id.");

		microsoftAuth.ClientSecret = builder.Configuration["MicrosoftOAuth:ClientSecret"]
			?? throw new ArgumentNullException("MicrosoftClientSecret", "Unable to find Microsoft OAuth client secret.");
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

builder.Services.AddMemoryCache(options =>
{
	options.SizeLimit = 150;
});

//Change application settings
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = new PathString("/auth/login");
	options.LogoutPath = new PathString("/auth/logout");
	options.LogoutPath = new PathString("/auth/logout");
	options.AccessDeniedPath = new PathString("/auth/noentry");

	options.ExpireTimeSpan = TimeSpan.FromDays(5);
	options.SlidingExpiration = true;

	options.ReturnUrlParameter = "return";

	options.Cookie.HttpOnly = true;
	options.Cookie.Name = "ApplicationUserIdentity";
	options.Cookie.IsEssential = true;

	//https://github.com/dotnet/aspnetcore/issues/13632.
	//Due to the SameSite attribute of the antiforgery cookie you can not reset password on one page
	//and login on another, you'll just get a 400(badrequest) because antiforgery token on the first
	//page will not match the antiforgery cookie (that has been changed by the second page).
});

builder.Services.ConfigureExternalCookie(options =>
{
	options.LoginPath = new PathString("/auth/login");
	options.LogoutPath = new PathString("/auth/logout");
	options.LogoutPath = new PathString("/auth/logout");
	options.AccessDeniedPath = new PathString("/auth/noentry");

	options.ExpireTimeSpan = TimeSpan.FromDays(5);
	options.SlidingExpiration = true;

	options.ReturnUrlParameter = "return";

	options.Cookie.HttpOnly = true;
	options.Cookie.Name = "ExternalUserIdentity";
	options.Cookie.IsEssential = true;
});

builder.Services.Configure<UserInteractionOptions>(
	builder.Configuration.GetSection(UserInteractionOptions.FieldName)
);

builder.Services.Configure<SMTPCredentialsOptions>(
	builder.Configuration.GetSection(SMTPCredentialsOptions.FieldName)
);

builder.Services.Configure<EmailConnectionInformationOptions>(
	builder.Configuration.GetSection(EmailConnectionInformationOptions.FieldName)
);

builder.Services.Configure<AntiforgeryOptions>(options =>
{
	options.Cookie.Name = "AntiForgeryToken";
	options.Cookie.IsEssential = true;
});

builder.Services.Configure<MicrosoftAccountOptions>(options =>
{
	options.AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
	options.TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
});

//Add custom services.
builder.Services
	.AddTransient<EmailSender>()
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

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/error");
	app.UseHsts();
}
else
{
	try
	{
		AuthConfiguration authConfig = new();
		app.Configuration.GetSection(AuthConfiguration.FieldName).Bind(authConfig);

		using (var scope = app.Services.CreateScope())
		{
			DatabaseInitializer initializer = new(
				scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
				scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>(),
				app.Configuration
			);

			await initializer.InitializeAuthAsync(authConfig);
		}
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
