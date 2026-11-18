using System.Net;
using System.Net.Http.Json;
using RedShelter.Tests.Infrastructure;

namespace RedShelter.Tests.Shelters;

public class AnimalEndpointTest
{
    [Fact]
    public async Task CreateAShelter()
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var request = new
        {
            Name = "Red Patitas",
            City = "Sonsonate"
        };

        // Act
        var response = await client.PostAsJsonAsync("api/shelters", request);

        // Equal
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var shelter = await response.Content.ReadFromJsonAsync<ShelterResponse>();
        Assert.NotNull(shelter);
        Assert.Equal("Red Patitas", shelter.Name);
        Assert.Equal("Sonsonate", shelter.City);

        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task AssignShelter_WhenBothExist_AssignsAnimal()
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var requestShelter = new
        {
            Name = "Red Patitas",
            City = "Sonsonate"
        };

        var requestAnimal = new
        {
            Name = "Canela",
            Species = "Gato"
        };

        // Act
        var responseAnimal = await client.PostAsJsonAsync("api/animals", requestAnimal);
        var responseShelter = await client.PostAsJsonAsync("api/shelters", requestShelter);

        Assert.Equal(HttpStatusCode.Created, responseAnimal.StatusCode);
        Assert.Equal(HttpStatusCode.Created, responseShelter.StatusCode);

        var animal = await responseAnimal.Content.ReadFromJsonAsync<AnimalResponse?>();
        var shelter = await responseShelter.Content.ReadFromJsonAsync<ShelterResponse?>();

        Assert.NotNull(animal);
        Assert.NotNull(shelter);

        var responseAssign = await client.PatchAsJsonAsync($"api/animals/{animal.Id}/shelter", new { shelterId = shelter.Id });

        Assert.Equal(HttpStatusCode.NoContent, responseAssign.StatusCode);
        // Assert

        var responseGet = await client.GetAsync($"/api/shelters/{shelter.Id}/animals");

        Assert.Equal(HttpStatusCode.OK,responseGet.StatusCode);

        var animals = await responseGet.Content.ReadFromJsonAsync<List<AnimalResponse>>();

        Assert.NotNull(animals);

        Assert.Contains(animals, a => a.Id == animal.Id);
    }
}