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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();

    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

    await seeder.SeedAsync(db);
}

app.UseContentHubMiddleware();

app.UseContentHubSwagger();

app.UseHttpsRedirection();

var storagePath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "storage",
    "assets");

Directory.CreateDirectory(storagePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/assets"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapContentHubHealthChecks();

app.MapEndpoints();

app.Run();
