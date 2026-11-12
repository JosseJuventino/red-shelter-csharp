using System.ComponentModel.DataAnnotations;
using RedShelter.Api.Features.Shelters;

namespace RedShelter.Api.Features.Animals;

public class Animal
{
    public Guid Id {get; set; }

    [MaxLength(50)]
    public string Name {get; set;} = string.Empty;
    
    public string Species { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public Guid? ShelterId { get; set; }

    public Shelter? Shelter { get; set; }
}