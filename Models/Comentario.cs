namespace ClasificadorComentarios.Models;

public class Comentario
{
    public int Id { get; set; }
    public string Usuario { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string Texto { get; set; } = "";
    public int Reportes { get; set; }
}
