7# RedShelter

Sistema de gestión de animales y refugios. Es un CRUD completo pensado como **proyecto de referencia**: sirve para consultar rápidamente cómo se arma una solución .NET de punta a punta (API + base de datos + cliente de escritorio + pruebas) cuando hay que construir algo parecido desde cero.

| Capa | Tecnología |
| --- | --- |
| API REST | ASP.NET Core 10 (controllers) + OpenAPI / Swagger UI |
| Persistencia | Entity Framework Core 10 + SQL Server |
| Escritorio | WPF (.NET 10) + MVVM con CommunityToolkit.Mvvm |
| Pruebas | xUnit + `WebApplicationFactory` + SQLite en memoria |

---

## Índice

1. [Estructura del repositorio](#1-estructura-del-repositorio)
2. [Correr el proyecto](#2-correr-el-proyecto)
3. [Endpoints de la API](#3-endpoints-de-la-api)
4. [Guía: crear una solución igual desde cero](#4-guía-crear-una-solución-igual-desde-cero)
5. [Receta: CRUD en la API](#5-receta-crud-en-la-api)
6. [Receta: cliente WPF con MVVM](#6-receta-cliente-wpf-con-mvvm)
7. [Receta: pruebas de integración](#7-receta-pruebas-de-integración)
8. [Chuleta de comandos](#8-chuleta-de-comandos)
9. [Errores comunes](#9-errores-comunes)

---

## 1. Estructura del repositorio

```
RedShelter.slnx
├── src/
│   ├── RedShelter.Api/                      # API REST
│   │   ├── Program.cs                       # DI, DbContext, Swagger, pipeline
│   │   ├── Features/
│   │   │   ├── Animals/                     # Entidad, DTOs, Service, Controller
│   │   │   └── Shelters/
│   │   ├── Infrastructure/Persistence/
│   │   │   └── RedShelterDbContext.cs       # DbSets y relaciones
│   │   └── Migrations/                      # Generadas por EF Core
│   └── RedShelter.Desktop/                  # Cliente WPF
│       ├── Models/                          # DTOs que viajan por HTTP
│       ├── Services/AnimalApiService.cs     # HttpClient → API
│       ├── ViewModels/                      # Lógica de la UI (MVVM)
│       ├── Views/                           # Ventanas secundarias
│       └── MainWindow.xaml                  # Ventana principal
└── tests/
    └── RedShelter.Tests/
        ├── Infrastructure/RedShelterApiFactory.cs   # API de prueba con SQLite
        └── Infrastructure/Animals|Shelters/         # Pruebas de endpoints
```

La API está organizada **por feature** (`Features/Animals`, `Features/Shelters`): todo lo de un recurso vive junto. Cada feature sigue el mismo patrón:

```
Entidad (Animal.cs)                 → tabla en la BD
Request DTOs (Create/Update...)     → lo que entra por HTTP, con validaciones
Response DTO (AnimalResponse.cs)    → lo que sale por HTTP
Service (AnimalService.cs)          → lógica + acceso a datos con EF Core
Controller (AnimalController.cs)    → rutas HTTP y códigos de estado
```

---

## 2. Correr el proyecto

### Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server (Express, LocalDB, Developer o en Docker)
- Herramienta de EF Core:

  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2.1 Configurar la cadena de conexión

La cadena de conexión **no está en `appsettings.json`**: se guarda con *user-secrets* para no subir credenciales al repositorio. `Program.cs` la lee con `GetConnectionString("DefaultConnection")`.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<cadena>" --project src/RedShelter.Api
```

Ejemplos de `<cadena>` según dónde esté SQL Server:

| Instalación | Cadena de conexión |
| --- | --- |
| Instancia local por defecto | `Server=localhost;Database=RedShelter;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server Express | `Server=localhost\SQLEXPRESS;Database=RedShelter;Trusted_Connection=True;TrustServerCertificate=True` |
| LocalDB | `Server=(localdb)\MSSQLLocalDB;Database=RedShelter;Trusted_Connection=True` |
| Docker (usuario `sa`) | `Server=localhost,1433;Database=RedShelter;User Id=sa;Password=<tu-password>;TrustServerCertificate=True` |

Para revisar lo guardado: `dotnet user-secrets list --project src/RedShelter.Api`.

> Alternativa rápida (sin user-secrets): agregar en `appsettings.Development.json`
> `"ConnectionStrings": { "DefaultConnection": "<cadena>" }`. No lo subas si tiene contraseña.

### 2.2 Crear la base de datos

```bash
dotnet ef database update --project src/RedShelter.Api
```

Aplica las migraciones de [src/RedShelter.Api/Migrations/](src/RedShelter.Api/Migrations/) y crea las tablas `Animals` y `Shelters`.

### 2.3 Levantar la API

```bash
dotnet run --project src/RedShelter.Api
```

- API: `http://localhost:5222`
- Swagger UI: `http://localhost:5222/swagger`
- Documento OpenAPI: `http://localhost:5222/openapi/v1.json`

### 2.4 Levantar la app de escritorio

Con la API corriendo, en otra terminal:

```bash
dotnet run --project src/RedShelter.Desktop
```

La URL de la API está fija en [MainWindow.xaml.cs](src/RedShelter.Desktop/MainWindow.xaml.cs) (`http://localhost:5222/`). Si cambiás el puerto de la API, cambialo ahí también.

### 2.5 Correr las pruebas

```bash
dotnet test
```

Las pruebas **no necesitan SQL Server**: reemplazan la base por SQLite en memoria (ver [sección 7](#7-receta-pruebas-de-integración)).

---

## 3. Endpoints de la API

### Animales — `api/animals`

| Método | Ruta | Body | Respuesta |
| --- | --- | --- | --- |
| `GET` | `/api/animals` | — | `200` lista de animales |
| `GET` | `/api/animals/{id}` | — | `200` animal · `404` |
| `POST` | `/api/animals` | `{ "name": "Luna", "species": "Gato" }` | `201` animal + header `Location` · `400` |
| `PATCH` | `/api/animals/{id}` | `{ "name": "Lunita" }` (campos opcionales) | `204` · `404` |
| `PATCH` | `/api/animals/{id}/shelter` | `{ "shelterId": "<guid>" }` | `204` · `400` · `404` |
| `DELETE` | `/api/animals/{id}` | — | `204` · `404` |

### Refugios — `api/shelters`

| Método | Ruta | Body | Respuesta |
| --- | --- | --- | --- |
| `GET` | `/api/shelters` | — | `200` lista de refugios |
| `GET` | `/api/shelters/{id}` | — | `200` refugio · `404` |
| `GET` | `/api/shelters/{id}/animals` | — | `200` animales del refugio · `404` |
| `POST` | `/api/shelters` | `{ "name": "Refugio Norte", "city": "San Salvador" }` | `201` refugio · `400` |

### Probar a mano con un archivo `.http`

VS Code (extensión REST Client) y Visual Studio ejecutan archivos `.http`. Ejemplo para [RedShelter.Api.http](src/RedShelter.Api/RedShelter.Api.http):

```http
@host = http://localhost:5222

### Listar
GET {{host}}/api/animals

### Crear
POST {{host}}/api/animals
Content-Type: application/json

{ "name": "Luna", "species": "Gato" }

### Editar (reemplazá el id)
PATCH {{host}}/api/animals/00000000-0000-0000-0000-000000000000
Content-Type: application/json

{ "name": "Lunita" }

### Eliminar
DELETE {{host}}/api/animals/00000000-0000-0000-0000-000000000000
```

Con `curl` (PowerShell: usar `curl.exe`):

```bash
curl -X POST http://localhost:5222/api/animals -H "Content-Type: application/json" -d "{\"name\":\"Luna\",\"species\":\"Gato\"}"
```

---

## 4. Guía: crear una solución igual desde cero

Reemplazá `DemoCrud` por el nombre del proyecto.

```bash
# Solución (en .NET 10 genera un .slnx)
dotnet new sln -n DemoCrud

# Proyectos
dotnet new webapi -n DemoCrud.Api     -o src/DemoCrud.Api     --use-controllers -f net10.0
dotnet new wpf    -n DemoCrud.Desktop -o src/DemoCrud.Desktop -f net10.0-windows
dotnet new xunit  -n DemoCrud.Tests   -o tests/DemoCrud.Tests -f net10.0

# Agregarlos a la solución
dotnet sln add src/DemoCrud.Api/DemoCrud.Api.csproj
dotnet sln add src/DemoCrud.Desktop/DemoCrud.Desktop.csproj
dotnet sln add tests/DemoCrud.Tests/DemoCrud.Tests.csproj

# Paquetes de la API
dotnet add src/DemoCrud.Api package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.15
dotnet add src/DemoCrud.Api package Microsoft.EntityFrameworkCore.Design   --version 10.0.15
dotnet add src/DemoCrud.Api package Swashbuckle.AspNetCore.SwaggerUI       --version 11.0.1

# Paquetes del escritorio
dotnet add src/DemoCrud.Desktop package CommunityToolkit.Mvvm --version 8.4.2

# Paquetes y referencia de las pruebas
dotnet add tests/DemoCrud.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/DemoCrud.Tests package Microsoft.EntityFrameworkCore.Sqlite
dotnet add tests/DemoCrud.Tests reference src/DemoCrud.Api/DemoCrud.Api.csproj

# Cadena de conexión
dotnet user-secrets init --project src/DemoCrud.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<cadena>" --project src/DemoCrud.Api

# Verificar que todo compila
dotnet build
```

**Orden recomendado de trabajo** después del esqueleto:

1. Entidad → `DbContext` → registrar en `Program.cs`
2. `dotnet ef migrations add InitialCreate` → `dotnet ef database update`
3. DTOs → Service → Controller → probar en Swagger
4. Pruebas de integración
5. Cliente WPF

---

## 5. Receta: CRUD en la API

Ejemplo completo con la entidad `Animal`. Archivos reales en [src/RedShelter.Api/Features/Animals/](src/RedShelter.Api/Features/Animals/).

### 5.1 Entidad

```csharp
public class Animal
{
    public Guid Id { get; set; }                     // EF genera el Guid al insertar

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public Guid? ShelterId { get; set; }             // FK opcional (nullable)
    public Shelter? Shelter { get; set; }            // navegación
}
```

Del lado "uno" de la relación:

```csharp
public ICollection<Animal> Animals { get; set; } = new List<Animal>();
```

### 5.2 DbContext

```csharp
public class RedShelterDbContext : DbContext
{
    public RedShelterDbContext(DbContextOptions<RedShelterDbContext> options) : base(options) { }

    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<Shelter> Shelters => Set<Shelter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Animal>()
            .HasOne(a => a.Shelter)          // un animal tiene como máximo un refugio
            .WithMany(s => s.Animals)        // un refugio tiene muchos animales
            .OnDelete(DeleteBehavior.SetNull); // si se borra el refugio, ShelterId queda en null
    }
}
```


No hace falta `HasForeignKey()`: EF reconoce `ShelterId` por convención (`<Navegación>Id`).

## Transactiobs
```csharp
await using var transaction =
    await _context.Database.BeginTransactionAsync();

try
{
    producto.Stock -= 1;

    _context.Ventas.Add(new Venta
    {
        ProductoId = producto.Id,
        Total = producto.Precio
    });

    await _context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 5.3 Program.cs

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la conexión a SQL Server.");

builder.Services.AddDbContext<RedShelterDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddScoped<AnimalService>();   // Scoped: una instancia por request, igual que el DbContext
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "RedShelter API v1"));
}

app.MapControllers();
app.Run();

public partial class Program { }   // necesario para que las pruebas usen WebApplicationFactory<Program>
```

### 5.4 DTOs

Nunca se expone la entidad directamente: entra un *request* y sale un *response*.

```csharp
// Entrada: [ApiController] responde 400 automáticamente si falla la validación
public class CreateAnimalRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(60)]  public string Species { get; set; } = string.Empty;
}

// PATCH: todo opcional; null = "no tocar este campo"
public class UpdateAnimalRequest
{
    [MaxLength(100)] public string? Name { get; set; }
    [MaxLength(100)] public string? Species { get; set; }
}

// Salida
public record AnimalResponse(Guid Id, string Name, string Species, bool IsAvailable);
```

### 5.5 Service

```csharp
public class AnimalService
{
    private readonly RedShelterDbContext _context;
    public AnimalService(RedShelterDbContext context) => _context = context;

    // LEER: AsNoTracking + Select directo al DTO (solo trae las columnas necesarias)
    public Task<List<AnimalResponse>> GetAllAsync() =>
        _context.Animals.AsNoTracking()
            .Select(a => new AnimalResponse(a.Id, a.Name, a.Species, a.IsAvailable))
            .ToListAsync();

    public Task<AnimalResponse?> GetByIdAsync(Guid id) =>
        _context.Animals.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AnimalResponse(a.Id, a.Name, a.Species, a.IsAvailable))
            .FirstOrDefaultAsync();

    // CREAR
    public async Task<AnimalResponse> CreateAsync(CreateAnimalRequest request)
    {
        var animal = new Animal { Name = request.Name.Trim(), Species = request.Species.Trim() };
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();
        return new AnimalResponse(animal.Id, animal.Name, animal.Species, animal.IsAvailable);
    }

    // ACTUALIZAR parcial: FindAsync trae la entidad con tracking, se modifica y se guarda
    public async Task<bool> PatchAsync(Guid id, UpdateAnimalRequest request)
    {
        var animal = await _context.Animals.FindAsync(id);
        if (animal is null) return false;

        if (request.Name is not null)    animal.Name = request.Name.Trim();
        if (request.Species is not null) animal.Species = request.Species.Trim();

        await _context.SaveChangesAsync();
        return true;
    }

    // ELIMINAR
    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var animal = await _context.Animals.FindAsync(id);
        if (animal is null) return false;

        _context.Animals.Remove(animal);
        await _context.SaveChangesAsync();
        return true;
    }

    // Verificar existencia sin traer el objeto: AnyAsync
    public async Task<bool> AssignShelterAsync(Guid animalId, Guid shelterId)
    {
        var animal = await _context.Animals.FindAsync(animalId);
        if (animal is null) return false;
        if (!await _context.Shelters.AnyAsync(s => s.Id == shelterId)) return false;

        animal.ShelterId = shelterId;
        await _context.SaveChangesAsync();
        return true;
    }
}
```

Convención usada: el service devuelve `null` / `false` cuando no encuentra el recurso y el controller lo traduce a `404`.

### 5.6 Controller

```csharp
[ApiController]
[Route("api/animals")]
public class AnimalsController : ControllerBase
{
    private readonly AnimalService _animalService;
    public AnimalsController(AnimalService animalService) => _animalService = animalService;

    [HttpGet]
    public async Task<ActionResult<List<AnimalResponse>>> GetAll()
        => Ok(await _animalService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnimalResponse>> GetById(Guid id)
    {
        var animal = await _animalService.GetByIdAsync(id);
        return animal is null ? NotFound() : Ok(animal);
    }

    [HttpPost]
    public async Task<ActionResult<AnimalResponse>> Create([FromBody] CreateAnimalRequest request)
    {
        var animal = await _animalService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = animal.Id }, animal);   // 201 + Location
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnimalRequest request)
        => await _animalService.PatchAsync(id, request) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _animalService.DeleteByIdAsync(id) ? NoContent() : NotFound();
}
```

| Operación | Éxito | No existe | Datos inválidos |
| --- | --- | --- | --- |
| GET lista | `200 Ok(lista)` | — | — |
| GET por id | `200 Ok(obj)` | `404 NotFound()` | — |
| POST | `201 CreatedAtAction(...)` | — | `400` (automático) |
| PATCH / PUT | `204 NoContent()` | `404 NotFound()` | `400` |
| DELETE | `204 NoContent()` | `404 NotFound()` | — |

### 5.7 Migración

```bash
dotnet ef migrations add InitialCreate --project src/RedShelter.Api
dotnet ef database update            --project src/RedShelter.Api
```

Cada vez que cambies una entidad o el `DbContext`: nueva migración con nombre descriptivo (`AddShelters`, `AddAgeToAnimal`...) y `database update`.

---

## 6. Receta: cliente WPF con MVVM

Archivos reales en [src/RedShelter.Desktop/](src/RedShelter.Desktop/).

```
MainWindow.xaml  ──Binding──▶  MainViewModel  ──▶  AnimalApiService  ──HTTP──▶  API
   (vista)                     (estado + comandos)    (HttpClient)
```

### 6.1 Models

Copias de los DTOs de la API (records simples). Los nombres de propiedad deben coincidir con el JSON.

```csharp
public record AnimalResponse(Guid Id, string Name, string Species, bool IsAvailable);
public record CreateAnimalRequest(string Name, string Species);
public record UpdateAnimalRequest(string? Name, string? Species);
```

### 6.2 Servicio HTTP

```csharp
public class AnimalApiService
{
    private readonly HttpClient _httpClient;
    public AnimalApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<AnimalResponse>> ObtenerAnimalesAsync()
        => await _httpClient.GetFromJsonAsync<List<AnimalResponse>>("api/animals") ?? new();

    public async Task<AnimalResponse> CrearAnimalAsync(CreateAnimalRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/animals", request);
        response.EnsureSuccessStatusCode();   // lanza HttpRequestException si no es 2xx
        return await response.Content.ReadFromJsonAsync<AnimalResponse>()
            ?? throw new JsonException("La API no devolvió un animal.");
    }

    public async Task ActualizarAnimalAsync(Guid id, UpdateAnimalRequest request)
    {
        using var response = await _httpClient.PatchAsJsonAsync($"api/animals/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task EliminarAnimalAsync(Guid id)
    {
        using var response = await _httpClient.DeleteAsync($"api/animals/{id}");
        response.EnsureSuccessStatusCode();
    }
}
```

### 6.3 Conectar todo en la ventana

```csharp
public MainWindow()
{
    InitializeComponent();

    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5222/") }; // con "/" final
    _viewModel = new MainViewModel(new AnimalApiService(httpClient));
    DataContext = _viewModel;                       // todos los {Binding} apuntan al ViewModel

    Loaded += async (_, _) => await _viewModel.CargarAnimalesCommand.ExecuteAsync(null);
}
```

### 6.4 ViewModel con CommunityToolkit.Mvvm

Los atributos generan el código repetitivo (`INotifyPropertyChanged`, `ICommand`). La clase debe ser `partial` y heredar de `ObservableObject`.

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly AnimalApiService _api;
    public MainViewModel(AnimalApiService api) => _api = api;

    // Lista que la UI observa: Add/Remove/Clear actualizan el DataGrid solos
    public ObservableCollection<AnimalResponse> Animales { get; } = new();

    // [ObservableProperty] → genera la notificación de cambio
    // [NotifyCanExecuteChangedFor] → re-evalúa si el botón se habilita
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    public partial string Nombre { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
    public partial string Especie { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EliminarAnimalCommand))]
    public partial AnimalResponse? AnimalSeleccionado { get; set; }

    [ObservableProperty]
    public partial string MensajeEstado { get; set; } = "Esperando conexión...";

    private bool PuedeGuardar() => !string.IsNullOrWhiteSpace(Nombre) && !string.IsNullOrWhiteSpace(Especie);
    private bool PuedeEliminar() => AnimalSeleccionado is not null;

    // [RelayCommand] sobre GuardarAsync → genera GuardarCommand (quita "Async")
    [RelayCommand(CanExecute = nameof(PuedeGuardar))]
    private async Task GuardarAsync()
    {
        try
        {
            var animal = await _api.CrearAnimalAsync(new CreateAnimalRequest(Nombre.Trim(), Especie.Trim()));
            Animales.Add(animal);
            Nombre = Especie = string.Empty;
            MensajeEstado = "Animal registrado correctamente.";
        }
        catch (HttpRequestException ex) { MensajeEstado = $"Error al guardar: {ex.Message}"; }
        catch (TaskCanceledException)   { MensajeEstado = "La API tardó demasiado en responder."; }
    }

    [RelayCommand(CanExecute = nameof(PuedeEliminar))]
    private async Task EliminarAnimalAsync()
    {
        var animal = AnimalSeleccionado!;
        if (MessageBox.Show($"¿Querés eliminar a {animal.Name}?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        await _api.EliminarAnimalAsync(animal.Id);
        Animales.Remove(animal);
        AnimalSeleccionado = null;
    }
}
```

### 6.5 XAML

```xml
<!-- Campo de texto enlazado en ambos sentidos, actualiza en cada tecla -->
<TextBox Text="{Binding Nombre, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- Botón: se deshabilita solo cuando CanExecute devuelve false -->
<Button Content="Guardar animal" Command="{Binding GuardarCommand}" />

<!-- Tabla enlazada a la colección y a la fila seleccionada -->
<DataGrid ItemsSource="{Binding Animales}"
          SelectedItem="{Binding AnimalSeleccionado, Mode=TwoWay}"
          AutoGenerateColumns="False" IsReadOnly="True" SelectionMode="Single">
    <DataGrid.Columns>
        <DataGridTextColumn     Header="Nombre"     Binding="{Binding Name}"        Width="*" />
        <DataGridTextColumn     Header="Especie"    Binding="{Binding Species}"     Width="*" />
        <DataGridCheckBoxColumn Header="Disponible" Binding="{Binding IsAvailable}" Width="*" />
    </DataGrid.Columns>
</DataGrid>

<StatusBar><TextBlock Text="{Binding MensajeEstado}" /></StatusBar>
```

### 6.6 Ventana de diálogo para editar

Patrón usado en [EditarAnimalWindow](src/RedShelter.Desktop/Views/EditarAnimalWindow.xaml.cs): la ventana tiene su propio ViewModel, y el ViewModel avisa con un evento cuando el usuario confirma.

```csharp
// EditarAnimalViewModel
public event Action? ConfirmacionSolicitada;

[RelayCommand(CanExecute = nameof(PuedeConfirmar))]
private void Confirmar() => ConfirmacionSolicitada?.Invoke();

// EditarAnimalWindow.xaml.cs
ViewModel = new EditarAnimalViewModel(animal);
DataContext = ViewModel;
ViewModel.ConfirmacionSolicitada += () => DialogResult = true;   // cierra la ventana devolviendo true

// MainViewModel: abrir y leer el resultado
var ventana = new EditarAnimalWindow(animal) { Owner = Application.Current.MainWindow };
if (ventana.ShowDialog() != true) return;   // canceló o cerró

await _api.ActualizarAnimalAsync(animal.Id,
    new UpdateAnimalRequest(ventana.ViewModel.Nombre.Trim(), ventana.ViewModel.Especie.Trim()));
await CargarAnimalesCommand.ExecuteAsync(null);
```

En el XAML del diálogo, `IsCancel="True"` en el botón Cancelar lo cierra con `DialogResult = false` sin código extra.

---

## 7. Receta: pruebas de integración

Las pruebas levantan la API real en memoria y le hacen peticiones HTTP. La base SQL Server se reemplaza por **SQLite en memoria**, así corren en cualquier máquina.

### 7.1 Fábrica de la API de prueba

[RedShelterApiFactory.cs](tests/RedShelter.Tests/Infrastructure/RedShelterApiFactory.cs):

```csharp
public class RedShelterApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // 1. Quitar la configuración de SQL Server
            services.RemoveAll<RedShelterDbContext>();
            services.RemoveAll<DbContextOptions<RedShelterDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RedShelterDbContext>>();

            // 2. Una conexión SQLite en memoria abierta (la BD vive mientras la conexión esté abierta)
            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection("Data Source=:memory:");
                connection.Open();
                return connection;
            });

            // 3. Registrar el DbContext usando esa conexión
            services.AddDbContext<RedShelterDbContext>((provider, options) =>
                options.UseSqlite(provider.GetRequiredService<DbConnection>()));
        });
    }

    public async Task<HttpClient> CreateTestClientAsync()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RedShelterDbContext>().Database.EnsureCreatedAsync();
        return client;
    }
}
```

### 7.2 Ejemplos de pruebas (Arrange / Act / Assert)

```csharp
[Fact]
public async Task Create_WithValidData_Returns201()
{
    using var factory = new RedShelterApiFactory();
    using var client = await factory.CreateTestClientAsync();

    var response = await client.PostAsJsonAsync("/api/animals", new { Name = "Luna", Species = "Gato" });

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var animal = await response.Content.ReadFromJsonAsync<AnimalResponse>();
    Assert.NotNull(animal);
    Assert.Equal("Luna", animal.Name);
    Assert.NotNull(response.Headers.Location);
}

// [Theory] + [InlineData]: la misma prueba con varios datos
[Theory]
[InlineData("", "Gato")]
[InlineData("Luna", "")]
[InlineData(null, "Gato")]
public async Task Create_WithInvalidData_Returns400(string? name, string? species)
{
    using var factory = new RedShelterApiFactory();
    using var client = await factory.CreateTestClientAsync();

    var response = await client.PostAsJsonAsync("/api/animals", new { Name = name, Species = species });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

Casos mínimos que conviene cubrir por recurso: crear válido (`201`), crear inválido (`400`), obtener inexistente (`404`), actualizar existente y verificar con un `GET`, actualizar/eliminar inexistente (`404`).

---

## 8. Chuleta de comandos

| Para... | Comando |
| --- | --- |
| Compilar todo | `dotnet build` |
| Correr la API | `dotnet run --project src/RedShelter.Api` |
| Correr la API con recarga en caliente | `dotnet watch --project src/RedShelter.Api` |
| Correr el escritorio | `dotnet run --project src/RedShelter.Desktop` |
| Correr pruebas | `dotnet test` |
| Correr una sola prueba | `dotnet test --filter "FullyQualifiedName~Create_WithValidData"` |
| Nueva migración | `dotnet ef migrations add <Nombre> --project src/RedShelter.Api` |
| Aplicar migraciones | `dotnet ef database update --project src/RedShelter.Api` |
| Quitar la última migración (si no se aplicó) | `dotnet ef migrations remove --project src/RedShelter.Api` |
| Volver la BD a una migración anterior | `dotnet ef database update <NombreMigracion> --project src/RedShelter.Api` |
| Borrar la BD | `dotnet ef database drop --project src/RedShelter.Api` |
| Ver migraciones | `dotnet ef migrations list --project src/RedShelter.Api` |
| Ver el SQL de las migraciones | `dotnet ef migrations script --project src/RedShelter.Api` |
| Guardar un secreto | `dotnet user-secrets set "<clave>" "<valor>" --project src/RedShelter.Api` |

---

## 9. Errores comunes

| Síntoma | Causa probable | Solución |
| --- | --- | --- |
| `No se encontró la conexión a SQL Server.` al iniciar la API | Falta la cadena de conexión | Configurarla con user-secrets ([2.1](#21-configurar-la-cadena-de-conexión)) |
| `dotnet ef` no se reconoce | Falta la herramienta global | `dotnet tool install --global dotnet-ef` |
| `Your startup project doesn't reference Microsoft.EntityFrameworkCore.Design` | Falta el paquete Design en la API | `dotnet add src/RedShelter.Api package Microsoft.EntityFrameworkCore.Design` |
| `The certificate chain was issued by an authority that is not trusted` | SQL Server con certificado autofirmado | Agregar `TrustServerCertificate=True` a la cadena |
| `Invalid object name 'Animals'` | No se aplicaron las migraciones | `dotnet ef database update` |
| La app WPF muestra "Error HTTP: No se puede establecer una conexión..." | La API no está corriendo o el puerto no coincide | Levantar la API y revisar `BaseAddress` en `MainWindow.xaml.cs` |
| Las peticiones devuelven `404` con una ruta correcta | `BaseAddress` sin `/` final o ruta con `/` inicial mal combinada | Usar `BaseAddress = "http://host:puerto/"` y rutas relativas `"api/animals"` |
| El botón de WPF no se habilita nunca | Falta `[NotifyCanExecuteChangedFor]` en la propiedad | Agregarlo apuntando al comando correspondiente |
| `WebApplicationFactory<Program>` no compila en las pruebas | `Program` no es accesible | Agregar `public partial class Program { }` al final de `Program.cs` |
| `POST` devuelve `400` sin entrar al controller | Falló la validación de `[Required]` / `[MaxLength]` | Revisar el body; la respuesta incluye el detalle de los errores |
