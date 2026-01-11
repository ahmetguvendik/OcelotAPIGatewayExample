var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

// Service B GET endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Service = "Service B",
        Message = "Service B'den gelen GET isteği başarılı",
        Timestamp = DateTime.Now
    });
})
.WithName("GetServiceB");

app.Run();