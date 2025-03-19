using Backoffice.Web.Services;
using Backoffice.Infrastructure.Data;
using Backoffice.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Uygulama başlatılıyor...");
// Tüm modüllerin servis kayıtlarını yap
builder.Services.AddServiceRegistrations(builder.Configuration);
logger.LogInformation("Servis kayıtları tamamlandı.");
var app = builder.Build();
logger.LogInformation("Uygulama oluşturuldu.");
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
logger.LogInformation("HSTS ayarları yapıldı.");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
logger.LogInformation("Routing ayarları yapıldı.");

app.UseAuthentication();
app.UseAuthorization();
logger.LogInformation("Authentication ve Authorization ayarları yapıldı.");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

logger.LogInformation("Controller route ayarları yapıldı.");

// İzinleri seed et
using (var scope = app.Services.CreateScope())
{
    logger.LogInformation("İzinler seed ediliyor...");
    try
    {
        logger.LogInformation("Veritabanı başlatılıyor...");
        await DbInitializer.Initialize(app.Services);
        logger.LogInformation("Veritabanı başlatıldı.");
        var permissionSeedService = scope.ServiceProvider.GetRequiredService<PermissionSeedService>();
        await permissionSeedService.SeedPermissionsAsync();
        logger.LogInformation("İzinler seed edildi.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu.");
    }
}

app.Run();
logger.LogInformation("Uygulama başlatıldı.");