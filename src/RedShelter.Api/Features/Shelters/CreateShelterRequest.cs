using System.ComponentModel.DataAnnotations;

namespace RedShelter.Api.Features.Shelters;

public class CreateShelterRequest
{
    [Required]
    [MaxLength(100)]
    public string Name {get; set;} = String.Empty;

    [Required]
    [MaxLength(100)]
    public string City {get; set;} = String.Empty;
}