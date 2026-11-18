
using System.Net.Http;
using System.Windows;
using RedShelter.Desktop.Services;
using RedShelter.Desktop.ViewModels;

namespace RedShelter.Desktop;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5222/")
        };

        var animalService = new AnimalApiService(httpClient);

        _viewModel = new MainViewModel(animalService);

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await _viewModel.CargarAnimalesCommand.ExecuteAsync(null);
    }
}
