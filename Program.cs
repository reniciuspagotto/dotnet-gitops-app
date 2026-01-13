var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/info", () => new
{
    Name = "Your Name",
    Email = "your.email@example.com",
    Version = "1.0.0"
});

app.MapGet("/health", () => new { Status = "Healthy" });

app.Run();
