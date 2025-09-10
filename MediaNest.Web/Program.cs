using Blazored.Toast;
using MediaNest.Web;
using MediaNest.Web.AuthStateProvider;
using MediaNest.Web.Components;
using MediaNest.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazoredToast();

// Authentication & Authorization
builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// customs ervice
builder.Services.AddSingleton<SettingService>();
builder.Services.AddScoped<JSRuntimeService>();
builder.Services.AddOutputCache();


builder.Services.AddHttpClient<ApiClient>(client => {
    client.BaseAddress = new("https+http://apiservice");
});


var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions {  // for .vtt subtitle
    ServeUnknownFileTypes = true,
    DefaultContentType = "text/vtt"
});
AppState.AssetsFolder = builder.Configuration["AssetsFolder"];
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

