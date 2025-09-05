using MediaNest.ApiService.Endpoints;
using MediaNest.ApiService.Repositories;
using MediaNest.ApiService.Services;
using MediaNest.Web.Database;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();           // Add service defaults & Aspire client integrations.
builder.Services.AddProblemDetails();   // Add services to the container.
builder.Services.AddOpenApi();          // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Services
// ========================================================

// SQL server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => { 
    options.UseSqlServer(connectionString);
});
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();


var app = builder.Build();


app.UseExceptionHandler();              // Configure the HTTP request pipeline.

// Map Endpoints
// ========================================================
app.MapGet("/", () => "Halo");
app.MapGameEndpoints();              
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();        // add Scalar UI
}

app.MapDefaultEndpoints();

// Middleware
// ========================================================
app.Run();

