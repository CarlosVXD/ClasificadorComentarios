using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using ClasificadorComentarios.Models;
using ClasificadorComentarios.Services;
using Microsoft.Win32;

namespace ClasificadorComentarios;

public partial class MainWindow : Window
{
    private List<Resultado> _resultados = new();
    private string _filtro = "todos";

    public Categoria[] Categorias { get; } = Enum.GetValues<Categoria>();

    public MainWindow()
    {
        InitializeComponent();
        CargarArchivo(System.IO.Path.Combine(AppContext.BaseDirectory, "comentarios.json"));
    }

    private void CargarArchivo(string ruta)
    {
        try
        {
            var json = System.IO.File.ReadAllText(ruta);
            var comentarios = JsonSerializer.Deserialize<List<Comentario>>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            _resultados = comentarios.Select(Clasificador.Clasificar).ToList();
            TxtArchivo.Text = System.IO.Path.GetFileName(ruta);
            ActualizarVista();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo leer el archivo:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ActualizarVista()
    {
        TxtTodos.Text = _resultados.Count.ToString();
        TxtOk.Text = _resultados.Count(r => r.Categoria == Categoria.Ok).ToString();
        TxtSpam.Text = _resultados.Count(r => r.Categoria == Categoria.Spam).ToString();
        TxtRevisar.Text = _resultados.Count(r => r.Categoria == Categoria.Revisar).ToString();

        var vista = _filtro == "todos"
            ? _resultados
            : _resultados.Where(r => r.Categoria.ToString().Equals(_filtro, StringComparison.OrdinalIgnoreCase));

        Grid.ItemsSource = Ordenar(vista).ToList();
    }

    // Más graves primero; a igual gravedad, más reportes primero (en Revisar); luego por id.
    private static IEnumerable<Resultado> Ordenar(IEnumerable<Resultado> items) =>
        items.OrderByDescending(r => r.Gravedad)
             .ThenByDescending(r => r.Categoria == Categoria.Revisar ? r.Comentario.Reportes : 0)
             .ThenBy(r => r.Comentario.Id);

    private void CambiarCategoria_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox combo || combo.DataContext is not Resultado actual) return;
        if (combo.SelectedItem is not Categoria nueva || nueva == actual.Categoria) return;

        var i = _resultados.IndexOf(actual);
        if (i < 0) return;

        _resultados[i] = actual with
        {
            Categoria = nueva,
            Manual = true,
            Gravedad = nueva == Categoria.Revisar ? actual.Gravedad : 0,
            Motivos = new[] { $"Cambiado por el moderador (antes: {actual.Categoria})" }
        };

        // se refresca después, para no reconstruir la tabla en medio del evento
        Dispatcher.BeginInvoke(new Action(ActualizarVista));
    }

    private void Filtro_Checked(object sender, RoutedEventArgs e)
    {
        _filtro = (string)((RadioButton)sender).Tag;
        if (IsInitialized) ActualizarVista(); // durante InitializeComponent aún no existe la grilla
    }

    private void AbrirArchivo_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog { Filter = "Archivos JSON (*.json)|*.json" };
        if (dialogo.ShowDialog() == true) CargarArchivo(dialogo.FileName);
    }
}
