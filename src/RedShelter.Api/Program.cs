using Microsoft.EntityFrameworkCore;
using RedShelter.Api.Features.Animals;
using RedShelter.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la conexión a SQL Server."
    );

// Add services to the container.
builder.Services.AddDbContext<RedShelterDbContext>(
    options => options.UseSqlServer(connectionString)
);
// Se agregan los 
builder.Services.AddScoped<AnimalService>(); 
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
