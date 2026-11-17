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

        Grid.ItemsSource = _filtro == "todos"
            ? _resultados
            : _resultados.Where(r => r.Categoria.ToString().Equals(_filtro, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void Filtro_Checked(object sender, RoutedEventArgs e)
    {
        _filtro = (string)((RadioButton)sender).Tag;
        if (IsInitialized) ActualizarVista();
    }

    private void AbrirArchivo_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog { Filter = "Archivos JSON (*.json)|*.json" };
        if (dialogo.ShowDialog() == true) CargarArchivo(dialogo.FileName);
    }
}
