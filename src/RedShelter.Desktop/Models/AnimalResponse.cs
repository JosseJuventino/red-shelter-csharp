
namespace RedShelter.Desktop.Models;

public record AnimalResponse(
    Guid Id,
    string Name,
    string Species,
    bool IsAvailable
);
