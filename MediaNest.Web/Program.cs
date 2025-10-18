using Blazored.Toast;
using MediaNest.Shared.Database;
using MediaNest.Shared.Entities;
using MediaNest.Shared.Services;
using MediaNest.Shared.Services.Background;
using MediaNest.Web.Components;
using MediaNest.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazoredToast();
builder.Services.AddScoped<JSRuntimeService>();
// ========================================================================================
// Background Task Queue
// ========================================================================================
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();
// ========================================================================================
// Authentication & Authorization
// ========================================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.Name = "auth_token";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.MaxAge = TimeSpan.FromDays(1);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthService>();
// ========================================================================================
// MongoDB 
// ========================================================================================
builder.Services.AddSingleton<FileService>();
builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddMongoClient();
builder.Services.AddMongoCollection<Account>("Accounts");
builder.Services.AddMongoCollection<Comic>("Comics");
builder.Services.AddMongoCollection<Music>("Musics");
builder.Services.AddMongoCollection<Video>("Videos");
builder.Services.AddMongoCollection<ComicList>("ComicLists");
builder.Services.AddMongoCollection<MusicList>("MusicLists");
builder.Services.AddMongoCollection<VideoList>("VideoLists");
// ========================================================================================
// Main MediaNest Service
// ========================================================================================
builder.Services.AddScoped<ComicListService>();
builder.Services.AddScoped<ComicCartService>();
builder.Services.AddScoped<ComicService>();

builder.Services.AddScoped<EntityService<Music, MusicList>>();
builder.Services.AddScoped<EntityService<Video, VideoList>>();;
// ========================================================================================

var app = builder.Build();
// Seed Administrator
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider.GetRequiredService<AuthService>();
    await services.CreateSeedAdmin();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();


// ========================================================================================
// Config MediaFiles & static file
// ========================================================================================
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions {  // for .vtt subtitle
    ServeUnknownFileTypes = true,
    DefaultContentType = "text/vtt"
});

var appState = app.Services.GetRequiredService<FileService>();
if (Directory.Exists(appState.AssetsFolder)) {
    app.UseStaticFiles(new StaticFileOptions {
        FileProvider = new PhysicalFileProvider(appState.AssetsFolder),
        RequestPath = new PathString("/Assets"),
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
