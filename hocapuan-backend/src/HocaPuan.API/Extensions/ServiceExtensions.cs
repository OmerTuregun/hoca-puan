using System.Text;
using System.Net;
using HocaPuan.API.Configuration;
using HocaPuan.API.Services;
using HocaPuan.Core.Interfaces.Moderation;
using HocaPuan.Core.Interfaces.Services;
using HocaPuan.Data;
using HocaPuan.Services;
using HocaPuan.Services.Moderation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HocaPuan.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var accessTokenCookieName = config.GetValue<string>($"{AuthCookieSettings.SectionName}:AccessTokenCookieName")
            ?? "access_token";

        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            opts.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (string.IsNullOrEmpty(context.Token) &&
                        context.Request.Cookies.TryGetValue(accessTokenCookieName, out var cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfessorService, ProfessorService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IUniversityService, UniversityService>();
        services.AddSingleton<IBannedWordsProvider, FileBannedWordsProvider>();
        services.AddSingleton<IContentModerationService, ContentModerationService>();
        services.AddScoped<IYokPlaywrightScraperService, YokPlaywrightScraperService>();
        services.AddHttpClient<IYokScraperService, YokScraperService>(client =>
        {
            client.BaseAddress = new Uri("https://akademik.yok.gov.tr");
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = false,
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HocaPuan API",
                Version = "v1",
                Description = "Türkiye üniversitelerindeki hocaları değerlendirme platformu"
            });

            // JWT auth için Swagger UI'a kilit butonu ekle
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Örnek: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var corsSettings = config.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
        var origins = corsSettings.AllowedOrigins
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();

        var environment = config["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        if (origins.Length == 0 && environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            origins =
            [
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:3000",
                "http://localhost:4173"
            ];
        }

        services.AddCors(options =>
        {
            options.AddPolicy("HocaPuanCors", policy =>
            {
                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection AddAuthCookieAndCsrf(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<AuthCookieSettings>()
            .Bind(config.GetSection(AuthCookieSettings.SectionName))
            .PostConfigure(settings => ApplyCookieSecurityFromEnvironment(config, settings));

        services.AddSingleton<AuthCookieService>();

        var cookieSecure = ResolveCookieSecure(config) ?? false;
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.SecurePolicy = cookieSecure
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
        });

        return services;
    }

    private static void ApplyCookieSecurityFromEnvironment(IConfiguration config, AuthCookieSettings settings)
    {
        if (ResolveCookieSecure(config) is { } secure)
            settings.Secure = secure;

        var explicitSameSite = config[$"{AuthCookieSettings.SectionName}:SameSite"];
        if (string.IsNullOrWhiteSpace(explicitSameSite) &&
            config.GetValue<bool>("USE_HTTPS"))
        {
            settings.SameSite = "Strict";
        }
    }

    private static bool? ResolveCookieSecure(IConfiguration config)
    {
        if (bool.TryParse(config["COOKIE_SECURE"], out var cookieSecure))
            return cookieSecure;

        if (bool.TryParse(config["USE_HTTPS"], out var useHttps))
            return useHttps;

        var sectionValue = config.GetValue<bool?>($"{AuthCookieSettings.SectionName}:Secure");
        return sectionValue;
    }
}
