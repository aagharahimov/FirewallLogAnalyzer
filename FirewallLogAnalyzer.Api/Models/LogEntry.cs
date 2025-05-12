// FirewallLogAnalyzer.Api/Models/LogEntry.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For [Column] if needed
using System.Net; // For IPAddress type

namespace FirewallLogAnalyzer.Api.Models;

public class LogEntry
{
    [Key] // Primary Key
    public long Id { get; set; } // Using long for potentially many entries

    public DateTime Timestamp { get; set; }

    [Required]
    [MaxLength(45)] // Max length for IPv6, good for IPv4 too
    public string SourceIP { get; set; } = string.Empty;

    [Required]
    [MaxLength(45)]
    public string DestinationIP { get; set; } = string.Empty;

    public int? SourcePort { get; set; }

    public int? DestinationPort { get; set; }

    [Required]
    [MaxLength(50)] // e.g., "Allowed", "Denied", "Blocked by Rule X"
    public string Action { get; set; } = string.Empty;
    
    // --- GeoIP Enrichment Fields ---
    [MaxLength(100)]
    public string? SourceGeoCountry { get; set; }

    [MaxLength(100)]
    public string? SourceGeoCity { get; set; }

    public double? SourceLatitude { get; set; }

    public double? SourceLongitude { get; set; }

    [MaxLength(100)]
    public string? DestinationGeoCountry { get; set; }

    [MaxLength(100)]
    public string? DestinationGeoCity { get; set; }

    public double? DestinationLatitude { get; set; }

    public double? DestinationLongitude { get; set; }

    // Optional: Store IPAddress objects (not directly mapped to DB by default without converters)
    // [NotMapped]
    // public IPAddress? SourceIPAddress => IPAddress.TryParse(SourceIP, out var ip) ? ip : null;
    // [NotMapped]
    // public IPAddress? DestinationIPAddress => IPAddress.TryParse(DestinationIP, out var ip) ? ip : null;

    // --- Future Enrichment Fields (add later as per the 3-month plan) ---
    // public bool? IsKnownMaliciousSource { get; set; }
    // public string? ThreatIntelSourceDetails { get; set; }
    // public string? Protocol { get; set; } // If you can determine it
}