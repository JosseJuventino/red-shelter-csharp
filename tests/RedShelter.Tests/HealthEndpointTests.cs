using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using RedShelter.Tests.Infrastructure;

namespace RedShelter.Tests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        // Arrange preparar
        var url = "/api/health";

        // Ejecutar 
        var response = await _client.GetAsync(url);

        // Verificar 
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }


    [Theory]
    [InlineData(100, HttpStatusCode.Created)]
    [InlineData(101, HttpStatusCode.BadRequest)]
    public async Task Create_WithNameLength_ReturnsExpectedStatus(
        int length,
        HttpStatusCode expectedStatus)
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var request = new
        {
            Name = new string('A', length),
            Species = "Gato"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/animals",
            request);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }


    [Fact]
    public async Task Create_WithLongName_ReturnsValidationErrors()
    {
        // Arrange
        using var factory = new RedShelterApiFactory();
        using var client = await factory.CreateTestClientAsync();

        var request = new
        {
            Name = new string('A', 101),
            Species = "Gato"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/animals",
            request);

        // Assert: comprobar respuesta HTTP
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        // Convertir JSON a objeto C#
        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);

        // Comprobar que existe un error para Name
        Assert.Contains(
            problem.Errors,
            error => error.Key.Equals(
                "Name",
                StringComparison.OrdinalIgnoreCase)
                && error.Value.Length > 0
        );
    }


}