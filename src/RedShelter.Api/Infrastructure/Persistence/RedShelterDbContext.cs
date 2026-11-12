using Microsoft.EntityFrameworkCore;
using RedShelter.Api.Features.Animals;
using RedShelter.Api.Features.Shelters;

namespace RedShelter.Api.Infrastructure.Persistence;

public class RedShelterDbContext : DbContext
{
    public RedShelterDbContext(DbContextOptions<RedShelterDbContext> options) : base(options) { }

    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<Shelter> Shelters => Set<Shelter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shelter>(entity =>
        {
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Shelter) //porque un animal tiene un refugio como máximo.
                .WithMany(s => s.Animals) //Y un refugio varios animales
                .OnDelete(DeleteBehavior.SetNull); // No escribimos HasForeignKey() porque EF Core ya reconoce ShelterId.
        });
    }
}