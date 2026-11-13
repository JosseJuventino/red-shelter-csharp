using System.ComponentModel.DataAnnotations;

namespace RedShelter.Api.Features.Animals;


public record AssignShelterRequest(Guid ShelterId);