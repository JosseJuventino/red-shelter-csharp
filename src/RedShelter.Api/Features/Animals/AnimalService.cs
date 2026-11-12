using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RedShelter.Api.Infrastructure.Persistence;

namespace RedShelter.Api.Features.Animals;

public class AnimalService
{
    private readonly RedShelterDbContext _context;

    public AnimalService(RedShelterDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnimalResponse>> GetAllAsync()
    {
        return await _context.Animals
            .AsNoTracking()
            .Select(a => new AnimalResponse(
                a.Id,
                a.Name,
                a.Species,
                a.IsAvailable
            )).ToListAsync();
    }

    public async Task<AnimalResponse?> GetAllByIdAsync(Guid id)
    {
        return await _context.Animals
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AnimalResponse(
                a.Id,
                a.Name,
                a.Species,
                a.IsAvailable
            )).FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var animal = await _context.Animals.FindAsync(id);
        if(animal is null)
        {
            return false;
        }
        
         _context.Animals.Remove(animal);

        await _context.SaveChangesAsync();

        return true;
    }

    
public async Task<bool> PatchAsync(Guid id, UpdateAnimalRequest request)
{
    var animal = await _context.Animals.FindAsync(id);

    if (animal is null)
        return false;

    if (request.Name is not null)
        animal.Name = request.Name.Trim();

    if (request.Species is not null)
        animal.Species = request.Species.Trim();

    await _context.SaveChangesAsync();

    return true;
}


    public async Task<AnimalResponse> CreateAsync(CreateAnimalRequest request)
    {
        var animalName = request.Name.Trim();
        var animalSpecies = request.Species.Trim();

        var animal = new Animal
        {
            Name = animalName,
            Species = animalSpecies
        };

        _context.Animals.Add(animal);
        
        await _context.SaveChangesAsync();

        return new AnimalResponse(
            animal.Id,
            animal.Name,
            animal.Species,
            animal.IsAvailable
        );
    }
}