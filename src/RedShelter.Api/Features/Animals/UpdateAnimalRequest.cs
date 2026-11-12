using System.ComponentModel.DataAnnotations;

namespace RedShelter.Api.Features.Animals;

public class UpdateAnimalRequest
{
    [MaxLength(100)]
    public string? Name {get; set;}

    [MaxLength(100)]
    public string? Species {get; set;}
}