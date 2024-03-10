using ApiRinha.Persistencia;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateSlimBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POSTGRES")))
{
    builder.Configuration["ConnectionStrings:Default"] = Environment.GetEnvironmentVariable("POSTGRES");
}


builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

app.MapControllers();

app.Run();
