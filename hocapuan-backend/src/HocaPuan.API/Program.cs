using HocaPuan.API.Configuration;
using HocaPuan.API.Extensions;
using HocaPuan.API.Middleware;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using HocaPuan.Data.Seed;
using HocaPuan.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Debug = builder.Environment.IsDevelopment();
    options.TracesSampleRate = 0;
    options.AutoSessionTracking = true;
    options.Environment = builder.Environment.EnvironmentName;
});

// ─── Servisler ───────────────────────────────────────────────
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthCookieAndCsrf(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<ImportJobStore>();
builder.Services.AddSwagger();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddHocaPuanRateLimiting(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─── Uygulama pipeline ───────────────────────────────────────
var app = builder.Build();

// Otomatik migration + seed (test ortamında atlanır)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HocaPuan API v1");
        c.RoutePrefix = string.Empty; // Swagger ana sayfada açılır: http://localhost:5001
    });
}

app.UseForwardedHeaders();
app.UseCors("HocaPuanCors");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CsrfValidationMiddleware>();
app.UseRateLimiter();
app.MapControllers();

app.Run();

public partial class Program { }
