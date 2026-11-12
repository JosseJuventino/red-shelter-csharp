using System.ComponentModel.DataAnnotations;

namespace RedShelter.Api.Features.Animals;

public class CreateAnimalRequest
{
    [Required]
    [MaxLength(100)]
    public String Name {get; set;} = string.Empty;

    [Required]
    [MaxLength(100)]
    public String Species {get; set;} = string.Empty;
}