
using System.Windows;
using RedShelter.Desktop.Models;
using RedShelter.Desktop.ViewModels;

namespace RedShelter.Desktop.Views;

public partial class EditarAnimalWindow : Window
{
    public EditarAnimalViewModel ViewModel { get; }

    public EditarAnimalWindow(AnimalResponse animal)
    {
        InitializeComponent();

        ViewModel = new EditarAnimalViewModel(animal);

        DataContext = ViewModel;

        ViewModel.ConfirmacionSolicitada += () =>
        {
            DialogResult = true;
        };
    }
}
