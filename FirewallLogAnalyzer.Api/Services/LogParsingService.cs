// FirewallLogAnalyzer.Api/Services/LogParsingService.cs
using CsvHelper;
using CsvHelper.Configuration;
using FirewallLogAnalyzer.Api.Models;
using System.Globalization;
using System.Net; // For IPAddress parsing/validation

namespace FirewallLogAnalyzer.Api.Services;

public class LogParsingService
{
    public IEnumerable<LogEntry> ParseLogFile(Stream fileStream)
    {
        var entries = new List<LogEntry>();
        // It's good practice to dispose of the StreamReader and CsvReader
        using var reader = new StreamReader(fileStream);
        // Configure CsvHelper if your CSV has specific needs (e.g., no header, different delimiter)
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, // Assuming your CSV has a header
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null, // Handle missing fields gracefully if necessary
            HeaderValidated = null, // For more flexibility in header names
            PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", "") // Normalize headers
        };

        using var csv = new CsvReader(reader, csvConfig);

        // Manually map if column names in CSV don't exactly match LogEntry property names
        // or if you need custom conversion.
        // If names match (case-insensitive after normalization), auto-mapping often works.
        // For explicit mapping:
        csv.Context.RegisterClassMap<LogEntryMap>();

        try
        {
            entries = csv.GetRecords<LogEntry>().ToList();
        }
        catch (CsvHelper.HeaderValidationException ex)
        {
            // Log this error. Potentially re-throw or return empty list
            Console.WriteLine($"CSV Header Validation Error: {ex.Message}");
            // You might want to inspect ex.InvalidHeaders or ex.MissingHeaders
            throw; // Or handle more gracefully
        }
        catch (CsvHelper.ReaderException ex)
        {
            // Log this error. This can happen for various parsing issues.
            Console.WriteLine($"CSV Reader Error on line {ex.Context.Parser.Row}: {ex.Message}");
            // Potentially skip problematic rows or stop parsing
            throw; // Or handle
        }


        // Post-parsing validation/transformation if needed
        foreach (var entry in entries)
        {
            // Example: Validate IP Addresses
            if (!IPAddress.TryParse(entry.SourceIP, out _))
            {
                // Handle invalid IP - log, skip, or mark entry as invalid
                Console.WriteLine($"Invalid Source IP found: {entry.SourceIP}");
            }
            // Add more validation as needed
        }

        return entries;
    }
}

// Define a ClassMap for CsvHelper for explicit column mapping and type conversion
public sealed class LogEntryMap : ClassMap<LogEntry>
{
    public LogEntryMap()
    {
        // Match CSV column names (case-insensitive due to PrepareHeaderForMatch)
        // Ensure your CSV headers are "Timestamp", "Source IP", "Destination IP", etc.
        Map(m => m.Timestamp).Name("Timestamp"); // Or specific index: .Index(0)
        Map(m => m.SourceIP).Name("Source IP");
        Map(m => m.DestinationIP).Name("Destination IP");
        Map(m => m.SourcePort).Name("Source Port");
        Map(m => m.DestinationPort).Name("Destination Port");
        Map(m => m.Action).Name("Action");

        // If your timestamp format is not standard, you might need a TypeConverter
        // Example for a custom DateTime format:
        // Map(m => m.Timestamp).Name("Timestamp").TypeConverterOption.Format("yyyy-MM-dd HH:mm:ss,fff");
        // CsvHelper is usually good with common ISO 8601 formats.
        // Your example "1746038962.8603900" is a Unix timestamp with fractional seconds.
        // We need a custom converter for this.

        // For Unix timestamp with fractional seconds "1746038962.8603900"
        Map(m => m.Timestamp).Name("Timestamp").Convert(args =>
        {
            var timestampString = args.Row.GetField<string>("Timestamp");
            if (double.TryParse(timestampString, NumberStyles.Any, CultureInfo.InvariantCulture, out double unixTimestampSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds((long)unixTimestampSeconds)
                                     .AddMilliseconds((unixTimestampSeconds - (long)unixTimestampSeconds) * 1000)
                                     .UtcDateTime; // Assuming logs are in UTC
            }
            // Handle parsing failure: log, throw, or return a default
            Console.WriteLine($"Could not parse timestamp: {timestampString}");
            return DateTime.MinValue; // Or throw new Exception($"Could not parse timestamp: {timestampString}");
        });
    }
}