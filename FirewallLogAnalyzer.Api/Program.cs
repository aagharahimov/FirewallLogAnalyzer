// FirewallLogAnalyzer.Api/Program.cs
using FirewallLogAnalyzer.Api.Data; // Add this
using FirewallLogAnalyzer.Api.Services; // Add this for later
using Microsoft.EntityFrameworkCore;  // Add this

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Register your services
builder.Services.Configure<GeoIpOptions>(builder.Configuration.GetSection("GeoIp")); // Configure options
builder.Services.AddSingleton<GeoIpService>(); // GeoIpService can be a singleton as DatabaseReader is thread-safe
builder.Services.AddScoped<LogParsingService>(); // Or AddTransient/AddSingleton depending on need

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Optional: Seed database in development
    // using (var scope = app.Services.CreateScope())
    // {
    //     var services = scope.ServiceProvider;
    //     // var dbContext = services.GetRequiredService<ApplicationDbContext>();
    //     // dbContext.Database.EnsureCreated(); // Or use migrations
    //     // SeedData.Initialize(services); // A static class to seed data
    // }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();