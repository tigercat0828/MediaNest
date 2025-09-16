using MediaNest.ApiService.Services;
using MediaNest.Shared.Entities;
using MediaNest.Web.Database;
using MediaNest.Web.Endpoints;
using MediaNest.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();           // Add service defaults & Aspire client integrations.
builder.Services.AddProblemDetails();   // Add services to the container.
builder.Services.AddOpenApi();          // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// ========================================================
// Jwt Authentication
// ========================================================
var secret = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});
builder.Services.AddAuthorization();


/*
// ========================================================
// SQL Server
// ========================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlServer(connectionString);
});
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
*/


// ========================================================
// MongoDB
// ========================================================
builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddMongoClient();
builder.Services.AddMongoCollection<Account>("Accounts");
builder.Services.AddMongoCollection<Comic>("Comics");
builder.Services.AddScoped<ComicService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();      // <<<<<<<<<<<<<<<
// Seed Admin
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider.GetRequiredService<AuthService>();
    await services.SetSeedAdminIfNotExist();
}

// ========================================================
// 1. Error Handling
// ========================================================
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler();              // Configure the HTTP request pipeline.
    app.UseHsts();                          // HTTPS Strict Transport Security
}
// ========================================================
// 2. HTTPS / Static Files / Routing
// ========================================================
if (!app.Environment.IsEnvironment("Docker")) {
    app.UseHttpsRedirection();
}
// app.UseStaticFiles();
app.UseRouting();
// ========================================================
// 3. Authentication & Authorization
// ========================================================
app.UseAuthentication();
app.UseAuthorization();
// ========================================================
// 4. Map Endpoints
// ========================================================
app.MapGet("/", () => "api is running");
app.MapAuthEndpoints();
app.MapComicServiceEndpoints();
//app.MapGameEndpoints();

app.MapDefaultEndpoints();
// ========================================================
// 5. Scalar / OpenAPI
// ========================================================
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();        // add Scalar UI
}
// ========================================================
app.Run();

