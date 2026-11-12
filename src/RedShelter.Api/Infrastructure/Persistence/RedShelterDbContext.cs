using Microsoft.EntityFrameworkCore;
using RedShelter.Api.Features.Animals;

namespace RedShelter.Api.Infrastructure.Persistence;

public class RedShelterDbContext : DbContext
{
    public RedShelterDbContext(DbContextOptions<RedShelterDbContext> options) : base (options){}

    public DbSet<Animal> Animals => Set<Animal>();
}