using catApplication.Configuration;
using catApplication.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddScoped<ICatService, CatService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cat API V1");
    c.RoutePrefix = string.Empty; // To serve Swagger at the app's root (localhost:8080/)
});

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        db.Database.Migrate();
    }
    catch (Exception e)
    {
        Console.WriteLine($"An error occurred while migrating the database: {e.Message}");
    }
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();