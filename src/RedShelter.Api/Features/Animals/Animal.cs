using System.ComponentModel.DataAnnotations;

namespace RedShelter.Api.Features.Animals;

public class Animal
{
    public Guid Id {get; set; }

    [MaxLength(50)]
    public string Name {get; set;} = string.Empty;
    
    public string Species { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}