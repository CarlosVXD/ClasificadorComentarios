namespace ClasificadorComentarios.Models;

public enum Categoria { Ok, Spam, Revisar }

public record Resultado(
    Comentario Comentario,
    Categoria Categoria,
    IReadOnlyList<string> Motivos,
    int Gravedad = 0,
    bool Manual = false)
{
    public string MotivosTexto => string.Join(" · ", Motivos);

    public string TextoMostrado =>
        string.IsNullOrWhiteSpace(Comentario.Texto) ? "(sin texto)" : Comentario.Texto;

    public string Prioridad => Categoria != Categoria.Revisar ? ""
        : Gravedad >= 4 ? "Alta"
        : Gravedad == 3 ? "Media"
        : "Baja";
}
