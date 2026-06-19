using System.Net;

namespace HocaPuan.API.Http;

public static class ClientIpResolver
{
    public static string Resolve(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var first = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first))
                return first;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>Log için IPv4 son oktet maskeleme (KVKK/GDPR).</summary>
    public static string MaskForLog(string ip)
    {
        if (IPAddress.TryParse(ip, out var address))
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var parts = ip.Split('.');
                if (parts.Length == 4)
                    return $"{parts[0]}.{parts[1]}.{parts[2]}.xxx";
            }

            if (address.IsIPv4MappedToIPv6)
            {
                var mapped = address.MapToIPv4().ToString();
                var parts = mapped.Split('.');
                if (parts.Length == 4)
                    return $"{parts[0]}.{parts[1]}.{parts[2]}.xxx";
            }
        }

        return "xxx";
    }
}
