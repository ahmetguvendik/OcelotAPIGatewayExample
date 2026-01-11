var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

// Service A GET endpoint
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        Service = "Service A",
        Message = "Service A'den gelen GET isteği başarılı",
        Timestamp = DateTime.Now
    });
})
.WithName("GetServiceA");

// Service A Test endpoint
app.MapGet("/test", () =>
{
    return Results.Ok("Bu bir Service A testtir");
})
.WithName("GetServiceATest");

app.Run();