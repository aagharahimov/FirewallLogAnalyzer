// FirewallLogAnalyzer.Api/Services/LogParsingService.cs
using CsvHelper;
using CsvHelper.Configuration;
using FirewallLogAnalyzer.Api.Models;
using System.Globalization;
using System.Net;

namespace FirewallLogAnalyzer.Api.Services;

public class LogParsingService
{
    private readonly GeoIpService _geoIpService; // Inject GeoIpService
    private readonly ILogger<LogParsingService> _logger;

    // Update constructor
    public LogParsingService(GeoIpService geoIpService, ILogger<LogParsingService> logger)
    {
        _geoIpService = geoIpService;
        _logger = logger;
    }

    public IEnumerable<LogEntry> ParseLogFile(Stream fileStream)
    {
        var parsedEntries = new List<LogEntry>();
        using var reader = new StreamReader(fileStream);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", "")
        };

        using var csv = new CsvReader(reader, csvConfig);
        csv.Context.RegisterClassMap<LogEntryMap>();

        try
        {
            // Parse basic entries first
            parsedEntries = csv.GetRecords<LogEntry>().ToList();
        }
        catch (Exception ex) // Catching a broader exception temporarily for robust parsing
        {
            _logger.LogError(ex, "Error during CSV parsing in GetRecords");
            // Depending on requirements, you might re-throw, return empty, or try to recover
            // For now, let's return what might have been parsed if any, or an empty list.
            return Enumerable.Empty<LogEntry>(); // Or handle more gracefully
        }

        // Now enrich the parsed entries
        var enrichedEntries = new List<LogEntry>();
        foreach (var entry in parsedEntries)
        {
            // Validate basic entry (e.g., IPs - this could be more robust)
            if (!IPAddress.TryParse(entry.SourceIP, out _) || !IPAddress.TryParse(entry.DestinationIP, out _))
            {
                _logger.LogWarning("Skipping entry due to invalid IP format: Source={Source}, Dest={Dest}", entry.SourceIP, entry.DestinationIP);
                continue; // Skip entries with invalid IPs before GeoIP lookup
            }

            // Enrich with Source GeoIP
            var sourceGeo = _geoIpService.GetGeoData(entry.SourceIP);
            if (sourceGeo != null)
            {
                entry.SourceGeoCountry = sourceGeo.Country.Name;
                entry.SourceGeoCity = sourceGeo.City.Name;
                entry.SourceLatitude = sourceGeo.Location.Latitude;
                entry.SourceLongitude = sourceGeo.Location.Longitude;
            }

            // Enrich with Destination GeoIP
            var destGeo = _geoIpService.GetGeoData(entry.DestinationIP);
            if (destGeo != null)
            {
                entry.DestinationGeoCountry = destGeo.Country.Name;
                entry.DestinationGeoCity = destGeo.City.Name;
                entry.DestinationLatitude = destGeo.Location.Latitude;
                entry.DestinationLongitude = destGeo.Location.Longitude;
            }
            enrichedEntries.Add(entry);
        }
        return enrichedEntries;
    }
}

// LogEntryMap remains the same as before (handling basic CSV fields and timestamp)
// ... (rest of LogEntryMap and any custom converters for CSV parsing) ...
public sealed class LogEntryMap : ClassMap<LogEntry> // Keep this definition
{
    public LogEntryMap()
    {
        Map(m => m.Timestamp).Name("Timestamp").Convert(args =>
        {
            var timestampString = args.Row.GetField<string>("Timestamp");
            if (double.TryParse(timestampString, NumberStyles.Any, CultureInfo.InvariantCulture, out double unixTimestampSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds((long)unixTimestampSeconds)
                                     .AddMilliseconds((unixTimestampSeconds - (long)unixTimestampSeconds) * 1000)
                                     .UtcDateTime;
            }
            // Log error or throw:
            // Console.WriteLine($"Could not parse timestamp: {timestampString} at row {args.Row.Parser.Row}");
            // For now, returning MinValue to avoid breaking the whole parse for one bad timestamp.
            // Consider how to handle this more robustly (e.g., skip row, mark as invalid).
            return DateTime.MinValue;
        });

        Map(m => m.SourceIP).Name("Source IP");
        Map(m => m.DestinationIP).Name("Destination IP");

        // Handle nullable ports
        Map(m => m.SourcePort).Name("Source Port").TypeConverterOption.NullValues(string.Empty);
        Map(m => m.DestinationPort).Name("Destination Port").TypeConverterOption.NullValues(string.Empty);
        
        Map(m => m.Action).Name("Action");

        // GeoIP fields are NOT mapped from CSV, they are enriched later
    }
}