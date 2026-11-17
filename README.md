## Clasificación de comentarios (app de escritorio, WPF)

Lee `comentarios.json`, clasifica cada comentario en **ok**, **spam** o **revisar** y lo muestra en una ventana
con el conteo por categoría y filtros.

## Requisitos
- Windows 10/11
- [.NET 8 SDK]

## Instrucciones para ejecutar
```
Abrir terminal y asegurarse estar en la carpeta del proyecto
Luego en la termianl ejecutar `dotnet run` y automaticamente se abrirá el aplicativo
```
La app carga `comentarios.json` automáticamente. Con el boton de "Abrir otro archivo…" puedes cargar otro JSON con el mismo formato.
También se puede abrir la carpeta en Visual Studio y simplemente presionar F5.

## Enfoque: reglas con puntaje (sin IA)
- No necesita llaves ni conexión a internet, y cada decisión se puede explicar (la columna **Motivo** muestra por qué).
- Spam: frases típicas ("gana dinero", "compra seguidores", "oferta", "cupón"), dominios sospechosos,
  exceso de mayúsculas o de "!". Cada señal suma puntos; con 3 o más es spam.
- Revisar: insultos, amenazas, teléfonos (dato personal), texto vacío, comentarios de prueba, 3+ reportes,
  o señales de spam que no alcanzan el umbral (por ejemplo, una pregunta que menciona un dominio sospechoso).
- Ok: el resto.
- Prioridad: spam > revisar > ok.

## Limitaciones
No detecta sarcasmo ni contexto (ej. el comentario 10 pasa como "ok"). Las listas de palabras están en
`Services/Clasificador.cs` y son fáciles de ampliar.

## Estructura
```
Models/Comentario.cs, Models/Resultado.cs   datos
Services/Clasificador.cs                    reglas
MainWindow.xaml(.cs)                        interfaz
```
