using Microsoft.AspNetCore.Mvc;

namespace RedShelter.Api.Features.Animals;

[ApiController]
[Route("api/animals")]
public class AnimalsController : ControllerBase
{
    private readonly AnimalService _animalService;

    public AnimalsController(AnimalService animalService)
    {
        _animalService = animalService;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<AnimalResponse>>> GetAll()
    {
        var animals = await _animalService.GetAllAsync();
        return Ok(animals);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnimalResponse>> GetById(Guid id)
    {
        var animal = await _animalService.GetAllByIdAsync(id);
        if (animal is null) return NotFound();
        return Ok(animal);
    }

    [HttpDelete("{id: guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _animalService.DeleteByIdAsync(id);

        if (!deleted) return NotFound();

        return NoContent();
    }


    [HttpPost]
    public async Task<ActionResult<AnimalResponse>> Create([FromBody] CreateAnimalRequest request)
    {
        var animal = await _animalService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new {id = animal.Id},
            animal
        );
    }

}