using BochazoEtpWitsml.DataAccess;
using BochazoEtpWitsml.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var clientDistPath = Path.Combine(builder.Environment.ContentRootPath, "..", "witsml.client", "dist");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IWellsService, WellsService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddDbContext<WitsmlDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WitsmlData") ?? 
        "Server=.\\SQLExpress;Database=WitsmlData;Trusted_Connection=True;TrustServerCertificate=True;",
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

var app = builder.Build();

var staticFileProvider = Directory.Exists(clientDistPath)
    ? new Microsoft.Extensions.FileProviders.PhysicalFileProvider(clientDistPath)
    : null;

if (staticFileProvider != null)
{
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = staticFileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = staticFileProvider });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
if (staticFileProvider != null)
    app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = staticFileProvider });
else
    app.MapFallbackToFile("index.html");

app.Run();
