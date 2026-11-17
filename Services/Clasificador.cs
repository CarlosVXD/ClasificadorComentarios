using System.Text.RegularExpressions;
using ClasificadorComentarios.Models;

namespace ClasificadorComentarios.Services;

public static class Clasificador
{
    private const int UmbralSpam = 3;
    private const int UmbralReportes = 3;

    private static readonly string[] FrasesSpam =
        { "gana dinero", "compra seguidores", "oferta", "cupón", "cupon", "descuento" };

    private static readonly string[] TldSospechosos = { "xyz", "top", "click", "biz" };
    private static readonly string[] PalabrasDominio = { "oferta", "dinero", "gana", "gratis" };

    private static readonly string[] Insultos =
        { "idiota", "imbécil", "imbecil", "estúpido", "estupido", "ignorantes" };

    private static readonly string[] Amenazas =
        { "te voy a encontrar", "vas a ver lo que te pasa", "te voy a matar", "te voy a buscar" };

    private static readonly Regex Dominio =
        new(@"(?:https?://)?(?:www\.)?((?:[a-z0-9-]+\.)+([a-z]{2,}))", RegexOptions.IgnoreCase);

    private static readonly Regex Telefono =
        new(@"(\+?57[\s-]?)?\b3\d{2}[\s-]?\d{3}[\s-]?\d{4}\b");

    public static Resultado Clasificar(Comentario c)
    {
        var texto = (c.Texto ?? "").Trim();
        var minus = texto.ToLowerInvariant();

        // Clasificación automática por medio de puntos: spam u ok
        var puntos = 0;
        var spam = new List<string>();

        foreach (var frase in FrasesSpam.Where(minus.Contains))
        {
            puntos += 2;
            spam.Add($"Frase de spam: \"{frase}\"");
        }

        foreach (Match m in Dominio.Matches(minus))
        {
            var dominio = m.Groups[1].Value;
            var tld = m.Groups[2].Value;
            if (TldSospechosos.Contains(tld) || PalabrasDominio.Any(dominio.Contains))
            {
                puntos += 2;
                spam.Add($"Dominio sospechoso: {dominio}");
                break;
            }
        }

        var letras = texto.Count(char.IsLetter);
        if (letras >= 10 && texto.Count(char.IsUpper) / (double)letras > 0.5)
        {
            puntos += 1;
            spam.Add("Exceso de mayúsculas");
        }

        if (texto.Count(ch => ch == '!') >= 3)
        {
            puntos += 1;
            spam.Add("Exceso de signos de exclamación");
        }

        var reportes = c.Reportes >= UmbralReportes ? $"{c.Reportes} reportes de usuarios" : null;

        if (puntos >= UmbralSpam)
        {
            if (reportes != null) spam.Add(reportes);
            return new Resultado(c, Categoria.Spam, spam);
        }

        // Clasificación por señales de alerta: revisar u ok
        var revisar = new List<string>();

        if (texto.Length == 0) revisar.Add("Comentario vacío");
        if (Insultos.Any(minus.Contains)) revisar.Add("Posible insulto o lenguaje ofensivo");
        if (Amenazas.Any(minus.Contains)) revisar.Add("Posible amenaza");
        if (Telefono.IsMatch(texto)) revisar.Add("Contiene un número de teléfono (dato personal)");
        if (minus.Contains("comentario de prueba")) revisar.Add("Parece un comentario de prueba");
        if (reportes != null) revisar.Add(reportes);
        revisar.AddRange(spam); // spam que no alcanza el umbral, pero que puede ser sospechoso

        if (revisar.Count > 0) return new Resultado(c, Categoria.Revisar, revisar);

        return new Resultado(c, Categoria.Ok, new[] { "Sin señales de alerta" });
    }
}
