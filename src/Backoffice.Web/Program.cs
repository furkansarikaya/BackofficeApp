using Backoffice.Web.Services;
using Backoffice.Infrastructure.Data;
using Backoffice.Web.Extensions;
using Backoffice.Web.Middleware;
using Backoffice.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

// Initialize logger
var logger = LoggerFactory.Create(config => 
{
    config.AddConsole();
    config.AddDebug();
}).CreateLogger<Program>();

logger.LogInformation("Application starting...");

// Register all module services
builder.Services.AddServiceRegistrations(builder.Configuration);

// Add exception filter
builder.Services.AddScoped<ExceptionFilter>();

// Add MVC with controllers and views
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionFilter>();
})
.AddRazorRuntimeCompilation();

// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

var app = builder.Build();

app.UseRequestLogging();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use IP filtering middleware
app.UseIpFiltering();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed permissions
using (var scope = app.Services.CreateScope())
{
    logger.LogInformation("Seeding permissions...");
    try
    {
        logger.LogInformation("Initializing database...");
        await DbInitializer.Initialize(app.Services);
        
        var permissionSeedService = scope.ServiceProvider.GetRequiredService<PermissionSeedService>();
        await permissionSeedService.SeedPermissionsAsync();
        logger.LogInformation("Permissions seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

app.Run();
logger.LogInformation("Application started successfully");