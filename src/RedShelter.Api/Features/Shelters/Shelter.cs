using RedShelter.Api.Features.Animals;

namespace RedShelter.Api.Features.Shelters;

public class Shelter
{
    public Guid Id {get; set;} = new Guid();
    public string Name {get; set;} = String.Empty;
    public string City {get; set;} = String.Empty;
    public ICollection<Animal> Animals { get; set; } = new List<Animal>();
}