using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RedShelter.Desktop.Models;
using RedShelter.Desktop.Services;

namespace RedShelter.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AnimalApiService _animalApiService;

    public MainViewModel(AnimalApiService animalApiService)
    {
        _animalApiService = animalApiService;
    }

    public ObservableCollection<AnimalResponse> Animales { get; } = new();

    [ObservableProperty]
    public partial string MensajeEstado { get; set; } = "Esperando conexión...";

    // Propiedades que se mandarán
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    public partial string Nombre { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    public partial string Especie { get; set; } = string.Empty;


    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EliminarAnimalCommand))]
    public partial AnimalResponse? AnimalSeleccionado { get; set; }

    private bool PuedeEliminar()
    {
        return AnimalSeleccionado is not null;
    }

    private bool PuedeGuardar()
    {
        return !string.IsNullOrWhiteSpace(Nombre)
            && !string.IsNullOrWhiteSpace(Especie) && Nombre.Trim().Length < 50;
    }

    

    [RelayCommand]
    private void Saludar()
    {
        MensajeEstado = "¡Hola desde MVVM!";
    }


    [RelayCommand(CanExecute = nameof(PuedeGuardar))]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre) ||
            string.IsNullOrWhiteSpace(Especie))
        {
            MensajeEstado = "Nombre y especie son obligatorios.";
            return;
        }

        try
        {
            var request = new CreateAnimalRequest(
                Nombre.Trim(),
                Especie.Trim()
            );

            var animal = await _animalApiService.CrearAnimalAsync(request);

            Animales.Add(animal);

            Nombre = string.Empty;
            Especie = string.Empty;

            MensajeEstado = "Animal registrado correctamente.";
        }
        catch (HttpRequestException ex)
        {
            MensajeEstado = $"Error al guardar: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            MensajeEstado = "La API tardó demasiado en responder.";
        }
        catch (JsonException)
        {
            MensajeEstado = "La respuesta de la API no es válida.";
        }
    }


    [RelayCommand(CanExecute = nameof(PuedeEliminar))]
    private async Task EliminarAnimalAsync()
    {
        var animal = AnimalSeleccionado;

        if (animal is null)
            return;

        var confirmacion = MessageBox.Show(
            $"¿Querés eliminar a {animal.Name}?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (confirmacion != MessageBoxResult.Yes)
            return;

        try
        {
            await _animalApiService.EliminarAnimalAsync(animal.Id);

            Animales.Remove(animal);

            AnimalSeleccionado = null;

            MensajeEstado = $"Se eliminó a {animal.Name}.";
        }
        catch (HttpRequestException ex)
        {
            MensajeEstado = $"Error al eliminar: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            MensajeEstado = "La API tardó demasiado en responder.";
        }
    }


    [RelayCommand]
    private async Task CargarAnimalesAsync()
    {
        MensajeEstado = "Cargando animales...";

        try
        {
            var animales = await _animalApiService.ObtenerAnimalesAsync();

            Animales.Clear();

            foreach (var animal in animales)
            {
                Animales.Add(animal);
            }

            MensajeEstado = $"Se cargaron {Animales.Count} animales.";
        }
        catch (HttpRequestException ex)
        {
            MensajeEstado = $"Error HTTP: {ex.Message}";
        }
        catch (TaskCanceledException)
        {
            MensajeEstado = "La API tardó demasiado en responder.";
        }
        catch (JsonException)
        {
            MensajeEstado = "La API devolvió un JSON inválido.";
        }
    }
}
