// FirewallLogAnalyzer.Api/Controllers/LogsController.cs
using FirewallLogAnalyzer.Api.Data;
using FirewallLogAnalyzer.Api.Models;
using FirewallLogAnalyzer.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FirewallLogAnalyzer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly LogParsingService _parsingService;

    public LogsController(ApplicationDbContext context, LogParsingService parsingService)
    {
        _context = context;
        _parsingService = parsingService;
    }

    // POST: api/logs/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")] // Specify we expect file data
    public async Task<IActionResult> UploadLogFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded or file is empty.");
        }

        // Ensure it's a CSV file (optional, but good practice)
        if (Path.GetExtension(file.FileName).ToLowerInvariant() != ".csv")
        {
            return BadRequest("Invalid file type. Please upload a CSV file.");
        }

        try
        {
            IEnumerable<LogEntry> logEntries;
            using (var stream = file.OpenReadStream())
            {
                logEntries = _parsingService.ParseLogFile(stream);
            }

            if (!logEntries.Any())
            {
                return Ok("File processed, but no valid log entries found or file was empty after parsing.");
            }

            // Batching might be needed for very large files to avoid long transaction times
            // and excessive memory usage if _context.AddRange is not efficient enough.
            // For now, simple AddRange.
            _context.LogEntries.AddRange(logEntries);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{logEntries.Count()} log entries processed and saved successfully." });
        }
        catch (Exception ex)
        {
            // Log the exception (use a proper logging framework like Serilog/NLog in a real app)
            Console.WriteLine($"Error uploading/processing file: {ex.Message} \n {ex.StackTrace}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/logs
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogEntry>>> GetLogEntries(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? sourceIpFilter = null,
        [FromQuery] string? destinationIpFilter = null,
        [FromQuery] string? actionFilter = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 200) pageSize = 200; // Max page size limit

        try
        {
            var query = _context.LogEntries.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(sourceIpFilter))
            {
                query = query.Where(log => log.SourceIP.Contains(sourceIpFilter));
            }
            if (!string.IsNullOrWhiteSpace(destinationIpFilter))
            {
                query = query.Where(log => log.DestinationIP.Contains(destinationIpFilter));
            }
            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                query = query.Where(log => log.Action.Equals(actionFilter, StringComparison.OrdinalIgnoreCase));
            }
            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                // Add 1 day to endDate if you want to include the whole end day
                query = query.Where(log => log.Timestamp < (endDate.HasValue ? endDate.Value.AddDays(1) : DateTime.MaxValue ));
            }


            // Order by timestamp descending (newest first) for typical log viewing
            query = query.OrderByDescending(log => log.Timestamp);

            var totalItems = await query.CountAsync();
            var logEntries = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // You might want to return pagination info in a custom response object or headers
            // Response.Headers.Add("X-Pagination-TotalCount", totalItems.ToString());
            // Response.Headers.Add("X-Pagination-PageSize", pageSize.ToString());
            // Response.Headers.Add("X-Pagination-CurrentPage", pageNumber.ToString());
            // Response.Headers.Add("X-Pagination-TotalPages", ((int)Math.Ceiling((double)totalItems / pageSize)).ToString());
            // Access-Control-Expose-Headers for custom headers if frontend is on different domain

            return Ok(new {
                TotalItems = totalItems,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Items = logEntries
            });
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Error getting log entries: {ex.Message} \n {ex.StackTrace}");
            return StatusCode(500, "Internal server error while retrieving log entries.");
        }
    }

    // GET: api/logs/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<LogEntry>> GetLogEntry(long id)
    {
        var logEntry = await _context.LogEntries.FindAsync(id);

        if (logEntry == null)
        {
            return NotFound();
        }

        return Ok(logEntry);
    }
}