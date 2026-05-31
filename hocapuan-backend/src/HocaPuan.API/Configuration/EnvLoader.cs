namespace HocaPuan.API.Configuration;

/// <summary>
/// Proje kökündeki .env dosyasını okur ve ASP.NET Core yapılandırma anahtarlarına eşler.
/// Docker Compose zaten ortam değişkenlerini geçirir; dotnet run için .env gerekir.
/// </summary>
public static class EnvLoader
{
    public static void Load()
    {
        var envPath = FindEnvFile(AppContext.BaseDirectory)
            ?? FindEnvFile(Directory.GetCurrentDirectory());

        if (envPath == null) return;

        foreach (var (key, value) in ParseEnvFile(envPath))
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
        }

        ApplyAspNetMappings();
        ApplyConnectionString();
    }

    private static string? FindEnvFile(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    private static IEnumerable<(string Key, string Value)> ParseEnvFile(string path)
    {
        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var eq = line.IndexOf('=');
            if (eq <= 0) continue;

            var key = line[..eq].Trim();
            var value = line[(eq + 1)..].Trim();
            if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
                value = value[1..^1];

            yield return (key, value);
        }
    }

    private static void ApplyAspNetMappings()
    {
        Map("JWT_SECRET", "JwtSettings__SecretKey");
        Map("JWT_ISSUER", "JwtSettings__Issuer");
        Map("JWT_AUDIENCE", "JwtSettings__Audience");
        Map("JWT_EXPIRATION_HOURS", "JwtSettings__ExpirationHours");

        Map("EMAIL_HOST", "Email__Host");
        Map("EMAIL_PORT", "Email__Port");
        Map("EMAIL_USERNAME", "Email__Username");
        Map("EMAIL_PASSWORD", "Email__Password");
        Map("EMAIL_FROM", "Email__From");

        Map("APP_FRONTEND_URL", "App__FrontendUrl");
    }

    private static void Map(string fromKey, string toKey)
    {
        var value = Environment.GetEnvironmentVariable(fromKey);
        if (string.IsNullOrWhiteSpace(value)) return;
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(toKey)))
            Environment.SetEnvironmentVariable(toKey, value);
    }

    private static void ApplyConnectionString()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")))
            return;

        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5433";
        var db = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (string.IsNullOrEmpty(db) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            return;

        var cs = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);
    }
}
