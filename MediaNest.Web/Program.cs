using Blazored.Toast;
using MediaNest.Web;
using MediaNest.Web.AuthStateProvider;
using MediaNest.Web.Components;
using MediaNest.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazoredToast();

// Persist Key for Docker
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("MediaNest");

// ====================================================================
// Authentication & Authorization
// ====================================================================
builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// ====================================================================
// customs service
// ====================================================================
builder.Services.AddSingleton<SettingService>();
builder.Services.AddScoped<JSRuntimeService>();
builder.Services.AddOutputCache();

var apiBaseUrl = builder.Configuration["ApiClient:BaseUrl"] ?? "https+http://apiservice";
Console.WriteLine($"Api BaseUrl : {apiBaseUrl}");
builder.Services.AddHttpClient<ApiClient>(client => {
    client.BaseAddress = new(apiBaseUrl);
});


var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseStaticFiles();
// ====================================================================
// Config MediaFiles
// ====================================================================
app.UseStaticFiles(new StaticFileOptions {  // for .vtt subtitle
    ServeUnknownFileTypes = true,
    DefaultContentType = "text/vtt"
});
var folder = builder.Configuration["AssetsFolder"] ?? "/app/Assets";
AppState.AssetsFolder = Path.GetFullPath(folder);
Console.WriteLine($"AssetsFolder : {AppState.AssetsFolder}");
string[] subDirs = { "Comics", "Videos", "Images" };
foreach (var dir in subDirs) {
    var path = Path.Combine(AppState.AssetsFolder, dir);
    Directory.CreateDirectory(path);
}
if (Directory.Exists(AppState.AssetsFolder)) {
    app.UseStaticFiles(new StaticFileOptions {
        FileProvider = new PhysicalFileProvider(AppState.AssetsFolder),
        RequestPath = new PathString("/Assets"),
    });
}

app.UseAntiforgery();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

