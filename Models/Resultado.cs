namespace ClasificadorComentarios.Models;

public enum Categoria { Ok, Spam, Revisar }

public record Resultado(Comentario Comentario, Categoria Categoria, IReadOnlyList<string> Motivos)
{
    public string MotivosTexto => string.Join(" · ", Motivos);

    public string TextoMostrado =>
        string.IsNullOrWhiteSpace(Comentario.Texto) ? "(sin texto)" : Comentario.Texto;
}
