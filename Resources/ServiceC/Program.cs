var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

// Service C GET endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Service = "Service C",
        Message = "Service C'den gelen GET isteği başarılı",
        Timestamp = DateTime.Now
    });
})
.WithName("GetServiceC");

app.Run();