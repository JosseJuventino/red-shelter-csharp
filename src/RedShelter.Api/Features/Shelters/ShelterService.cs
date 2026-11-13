using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedShelter.Api.Infrastructure.Persistence;

namespace RedShelter.Api.Features.Shelters;

public class ShelterService
{
    private readonly RedShelterDbContext _context;

    public ShelterService(RedShelterDbContext context)
    {
       _context = context;
    }

    public async Task<ShelterResponse> CreateAsync(CreateShelterRequest request)
    {
        var shelterName = request.Name.Trim();
        var shelterCity = request.City.Trim();

        var shelter = new Shelter
        {
            Name = shelterName,
            City = shelterCity
        };

        _context.Shelters.Add(shelter);

        await _context.SaveChangesAsync();

        return new ShelterResponse(
            shelter.Id,
            shelter.Name,
            shelter.City
        );
    }

    public async Task<ShelterResponse?> GetByIdAsync(Guid id)
    {
        return await _context.Shelters
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ShelterResponse(
                s.Id,
                s.Name,
                s.City
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<List<ShelterResponse>> GetAllAsync()
    {
        return await _context.Shelters
            .AsNoTracking()
            .Select(s => new ShelterResponse(
                s.Id,
                s.Name,
                s.City
            )).ToListAsync();
    }

    public async Task<List<AnimalResponse>?> AnimalResponseAsync(Guid shelterId)
    {
        // Devuelve true si existe al menos un refugio que cumple la condición.
        // Es diferente de FindAsync(), porque no necesitamos obtener todo el objeto
        var exists = await _context.Shelters.AnyAsync(s => s.Id == shelterId);
        if(!exists) return null;
        
        return await _context.Animals
            .AsNoTracking()
            .Where(a => a.ShelterId == shelterId)
            .Select(a => new AnimalResponse(
                a.Id,
                a.Name,
                a.Species,
                a.IsAvailable
            )).ToListAsync();
    }
}