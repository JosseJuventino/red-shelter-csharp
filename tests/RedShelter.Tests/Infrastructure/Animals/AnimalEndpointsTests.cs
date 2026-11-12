using System.Net;
using System.Net.Http.Json;
using RedShelter.Tests.Infrastructure;

namespace RedShelter.Tests.Animals;

public class AnimalEndpointTest
{
    [Fact]
    public async Task GetById_WhenNotFound_Returns404()
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var id = Guid.NewGuid();

        // Act 
        var response = await client.GetAsync($"/api/animals/{id}");

        //Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var request = new
        {
            Name = "Luna",
            Species = "Gato"
        };

        // Act
        var response = await client.PostAsJsonAsync("api/animals", request);

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var animal = await response.Content.ReadFromJsonAsync<AnimalResponse>();
        Assert.NotNull(animal);
        Assert.NotEqual(Guid.Empty, animal.Id);
        Assert.Equal("Luna", animal.Name);
        Assert.Equal("Gato", animal.Species);
        Assert.True(animal.IsAvailable);

        Assert.NotNull(response.Headers.Location);
    }


    [Fact]
    public async Task Update_WhenAnimalExists_UpdatesData()
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/api/animals",
            new { Name = "Luna", Species = "Gato" });

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<AnimalResponse>();

        Assert.NotNull(created);

        // Act
        var updateResponse = await client.PatchAsJsonAsync(
            $"/api/animals/{created.Id}",
            new { Name = "Lunita", Species = "Gato" });

        // Assert 
        Assert.Equal(
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        // Assert: verificar lo guardado
        var getResponse = await client.GetAsync(
            $"/api/animals/{created.Id}");

        getResponse.EnsureSuccessStatusCode();

        var updated = await getResponse.Content
            .ReadFromJsonAsync<AnimalResponse>();

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Lunita", updated.Name);
        Assert.Equal("Gato", updated.Species);
    }


    [Theory]
    [InlineData("", "Gato")]
    [InlineData("Luna", "")]
    [InlineData(null, "Gato")]
    [InlineData("Luna", null)]
    public async Task Create_WithInvalidData_Returns400(
        string? name,
        string? species)
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var request = new
        {
            Name = name,
            Species = species
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/animals",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task Patch_WhenAnimalDoesNotExist_Returns404()
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var id = Guid.NewGuid();

        var response = await client.PatchAsJsonAsync($"/api/animals/{id}", new { Name = "Max", Species = "Perro" });

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

}