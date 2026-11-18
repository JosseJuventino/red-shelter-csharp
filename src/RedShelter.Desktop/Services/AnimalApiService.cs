


using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RedShelter.Desktop.Models;

namespace RedShelter.Desktop.Services;

public class AnimalApiService
{
    private readonly HttpClient _httpClient;

    public AnimalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AnimalResponse>> ObtenerAnimalesAsync()
    {
        var animales = await _httpClient.GetFromJsonAsync<List<AnimalResponse>>("api/animals");

        return animales ?? new List<AnimalResponse>();
    }

    public async Task<AnimalResponse> CrearAnimalAsync(CreateAnimalRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/animals", request);
        response.EnsureSuccessStatusCode();

        var animal = await response.Content.ReadFromJsonAsync<AnimalResponse>();

        return animal ?? throw new JsonException("La api ndevoldio un animal");
    }
}
