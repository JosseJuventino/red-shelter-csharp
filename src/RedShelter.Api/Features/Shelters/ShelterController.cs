using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace RedShelter.Api.Features.Shelters;

[ApiController]
[Route("api/shelter")]
public class ShelterController : ControllerBase
{
    private readonly ShelterService _shelterService;

    public ShelterController(ShelterService shelterService)
    {
        _shelterService = shelterService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShelterResponse>> GetById(Guid id)
    {
        var shelter = await _shelterService.GetByIdAsync(id);

        if (shelter is null) return NotFound();

        return Ok(shelter);
    }

    [HttpPost]
    public async Task<ActionResult<ShelterResponse>> Create([FromBody] CreateShelterRequest request)
    {
        var shelter = await _shelterService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new {id = shelter.Id},
            shelter
        );
    }

    [HttpGet]
    public async Task<ActionResult<List<ShelterResponse>>> GetAll()
    {
        var shelters = await _shelterService.GetAllAsync();
        return Ok(shelters);
    }

    [HttpGet("{id:guid}/animals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AnimalResponse>?>> GetAnimals(Guid id)
    {
        var animals = await _shelterService.AnimalResponseAsync(id);
        if (animals is null) return NotFound();

        return Ok(animals);
    }
}