using HtmlToPdfService.Data;
using HtmlToPdfService.Helpers;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);

var app = builder.Build();

MigrateDatabase(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(ConfigureCors);
app.UseAuthorization();
app.MapControllers();
app.Run();

void ConfigureServices(IServiceCollection services)
{
    // Add services to the container.
    services.AddControllers();
    services.AddDbContext();
    services.AddServices();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

void ConfigureCors(CorsPolicyBuilder builder)
{
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod();
}

void MigrateDatabase(IServiceProvider services)
{
    using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        var db = services.GetRequiredService<IDbContextFactory<JobContext>>().CreateDbContext();
        db.Database.Migrate();
    }
}
