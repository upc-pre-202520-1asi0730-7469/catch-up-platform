using CatchUpPlatform.API.News.Application.Internal.CommandServices;
using CatchUpPlatform.API.News.Application.Internal.QueryServices;
using CatchUpPlatform.API.News.Domain.Repositories;
using CatchUpPlatform.API.News.Domain.Services;
using CatchUpPlatform.API.News.Infrastructure.Persistence.Repositories;
using CatchUpPlatform.API.Shared.Domain.Repositories;
using CatchUpPlatform.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using CatchUpPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using CatchUpPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Localization Configuration
builder.Services.AddLocalization();

builder.Services.AddControllers(options =>
        options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddDataAnnotationsLocalization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.EnableAnnotations());
builder.Services.AddOpenApi();

// Add Database Connection
if (builder.Environment.IsDevelopment())
    builder.Services.AddDbContext<AppDbContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString is null) 
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    options.UseMySQL(connectionString)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
});

// Configure Dependency Injection

// Shared Bounded Context Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// News Bounded Context Dependency Injection
builder.Services.AddScoped<IFavoriteSourceRepository, FavoriteSourceRepository>();
builder.Services.AddScoped<IFavoriteSourceCommandService, FavoriteSourceCommandService>();
builder.Services.AddScoped<IFavoriteSourceQueryService, FavoriteSourceQueryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Verify Database Objects Creation
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Localization Configuration
var supportedCultures = new[] { "en", "en-US", "es", "es-PE" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();