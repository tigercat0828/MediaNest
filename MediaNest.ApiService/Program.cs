using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();           // Add service defaults & Aspire client integrations.
builder.Services.AddProblemDetails();   // Add services to the container.
builder.Services.AddOpenApi();          // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


var app = builder.Build();


app.UseExceptionHandler();              // Configure the HTTP request pipeline.

// Map Endpoints
// ========================================================
app.MapGet("/", () => "Halo");

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.MapScalarApiReference();        // add Scalar UI
}


app.MapDefaultEndpoints();

// Middleware
// ========================================================
app.Run();

