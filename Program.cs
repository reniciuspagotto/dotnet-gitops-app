var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:8080");

app.MapGet("/api/info", () => new
{
    Name = "GitOps Demo App",
    Email = "demo@gitops.io",
    Version = "2.0.0",
    Timestamp = DateTime.UtcNow
});

app.MapGet("/health", () => new { Status = "Healthy" });

app.Run();
