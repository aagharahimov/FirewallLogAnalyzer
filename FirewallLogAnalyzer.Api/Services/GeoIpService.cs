// FirewallLogAnalyzer.Api/Services/GeoIpService.cs
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Options;
using System.Net;

namespace FirewallLogAnalyzer.Api.Services;

public class GeoIpOptions
{
    public string DbPath { get; set; } = string.Empty;
}

public class GeoIpService : IDisposable
{
    private readonly DatabaseReader? _reader;
    private readonly ILogger<GeoIpService> _logger;

    public GeoIpService(IOptions<GeoIpOptions> options, ILogger<GeoIpService> logger)
    {
        _logger = logger;
        var dbPath = options.Value.DbPath;

        if (string.IsNullOrWhiteSpace(dbPath))
        {
            _logger.LogError("GeoIP database path is not configured.");
            _reader = null; // Reader remains null if path is missing
            return;
        }

        // Resolve the path relative to the application's base directory
        var absoluteDbPath = Path.Combine(AppContext.BaseDirectory, dbPath);

        if (!File.Exists(absoluteDbPath))
        {
            _logger.LogError("GeoIP database file not found at: {Path}", absoluteDbPath);
            _reader = null; // Reader remains null if file not found
            return;
        }

        try
        {
            _reader = new DatabaseReader(absoluteDbPath);
            _logger.LogInformation("GeoIP DatabaseReader initialized successfully from: {Path}", absoluteDbPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing GeoIP DatabaseReader from: {Path}", absoluteDbPath);
            _reader = null; // Ensure reader is null on error
        }
    }

    public CityResponse? GetGeoData(string ipAddress)
    {
        if (_reader == null)
        {
            _logger.LogWarning("GeoIP DatabaseReader is not initialized. Cannot get GeoData for {IP}", ipAddress);
            return null;
        }

        if (string.IsNullOrWhiteSpace(ipAddress) || !IPAddress.TryParse(ipAddress, out var parsedIp))
        {
            return null; // Invalid IP
        }

        // Skip private/internal IPs as they usually won't be in the GeoIP database
        if (IsPrivateIpAddress(parsedIp))
        {
            return null;
        }

        try
        {
            return _reader.City(parsedIp);
        }
        catch (MaxMind.GeoIP2.Exceptions.AddressNotFoundException)
        {
            _logger.LogDebug("GeoIP address not found for {IP}", ipAddress);
            return null; // IP not found in the database
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up GeoIP data for {IP}", ipAddress);
            return null;
        }
    }

    private bool IsPrivateIpAddress(IPAddress ipAddress)
    {
        // Check for loopback
        if (IPAddress.IsLoopback(ipAddress)) return true;

        // IPv4 private ranges:
        // 10.0.0.0    - 10.255.255.255  (10/8 prefix)
        // 172.16.0.0  - 172.31.255.255 (172.16/12 prefix)
        // 192.168.0.0 - 192.168.255.255 (192.168/16 prefix)
        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            switch (bytes[0])
            {
                case 10:
                    return true;
                case 172:
                    return bytes[1] >= 16 && bytes[1] <= 31;
                case 192:
                    return bytes[1] == 168;
            }
        }
        // You could add IPv6 private ranges if necessary (e.g., fc00::/7 for Unique Local Addresses)
        // For now, focusing on common IPv4 firewall logs.
        return false;
    }


    public void Dispose()
    {
        _reader?.Dispose();
        GC.SuppressFinalize(this);
    }
}