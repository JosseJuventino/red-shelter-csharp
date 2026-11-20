
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RedShelter.Desktop.Models;

namespace RedShelter.Desktop.ViewModels;

public partial class EditarAnimalViewModel : ObservableObject
{
    public EditarAnimalViewModel(AnimalResponse animal)
    {
        Nombre = animal.Name;
        Especie = animal.Species;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarCommand))]
    public partial string Nombre { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarCommand))]
    public partial string Especie { get; set; }

    public event Action? ConfirmacionSolicitada;

    private bool PuedeConfirmar()
    {
        return !string.IsNullOrWhiteSpace(Nombre)
            && Nombre.Trim().Length <= 50
            && !string.IsNullOrWhiteSpace(Especie);
    }

    [RelayCommand(CanExecute = nameof(PuedeConfirmar))]
    private void Confirmar()
    {
        ConfirmacionSolicitada?.Invoke();
    }
}
