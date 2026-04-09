using WitsmlODViewer.DataAccess;
using WitsmlODViewer.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var clientDistPath = Path.Combine(builder.Environment.ContentRootPath, "..", "witsml.client", "dist");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WitsmlODViewer API (WITSML 1.4.1)",
        Version = "v1"
    });
});

builder.Services.AddScoped<IWellsService, WellsService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.Configure<AlarmEmailOptions>(builder.Configuration.GetSection(AlarmEmailOptions.SectionName));
builder.Services.AddSingleton<IAlarmEmailSender, AlarmEmailSender>();
builder.Services.AddScoped<IAlarmsService, AlarmsService>();
builder.Services.AddDbContext<Witsml141DataContext>(options =>
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
