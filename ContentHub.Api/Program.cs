using System.Reflection;
using ContentHub.Api.Common.EndpointDefinitions;
using ContentHub.Api.Extensions;
using ContentHub.Data;
using ContentHub.Data.Persistence;
using ContentHub.Data.Persistence.Seed;
using ContentHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddContentHubSwagger();
builder.Services.AddContentHubHealthChecks();
builder.Services.AddContentHubValidation();
builder.Services.AddContentHubData(builder.Configuration);
builder.Services.AddContentHubInfrastructure(builder.Configuration);
builder.Services.AddContentHubAuthentication(builder.Configuration);
builder.Services.AddContentHubAuthorization();
builder.Services.AddContentHubAuditing();
builder.Services.AddEndpointDefinitions(Assembly.GetExecutingAssembly());

var app = builder.Build();

var applyMigrationsOnStartup = app.Configuration.GetValue(
    "Database:ApplyMigrationsOnStartup",
    true);

var seedOnStartup = app.Configuration.GetValue(
    "Database:SeedOnStartup",
    true);

if (applyMigrationsOnStartup || seedOnStartup)
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

    if (applyMigrationsOnStartup)
    {
        await db.Database.MigrateAsync();
    }

    if (seedOnStartup)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

        await seeder.SeedAsync(db);
    }
}

app.UseContentHubMiddleware();

app.UseContentHubSwagger();

app.UseHttpsRedirection();

var storageRootPath =
    app.Configuration["Storage:LocalRootPath"] ??
    app.Configuration["Storage:RootPath"] ??
    "storage/assets";

var storagePath = Path.Combine(
    Directory.GetCurrentDirectory(),
    storageRootPath);

var storagePublicBaseUrl =
    app.Configuration["Storage:PublicBaseUrl"] ??
    "/assets";

Directory.CreateDirectory(storagePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = ResolveStorageRequestPath(storagePublicBaseUrl)
});

app.UseAuthentication();
app.UseAuthorization();

app.MapContentHubHealthChecks();

app.MapEndpoints();

app.Run();

static string ResolveStorageRequestPath(string publicBaseUrl)
{
    if (Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var absoluteUri) &&
        !string.IsNullOrWhiteSpace(absoluteUri.AbsolutePath))
    {
        return absoluteUri.AbsolutePath.TrimEnd('/');
    }

    var normalized = publicBaseUrl.TrimEnd('/');

    if (string.IsNullOrWhiteSpace(normalized))
    {
        return "/assets";
    }

    return normalized.StartsWith('/')
        ? normalized
        : $"/{normalized}";
}
