using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Prestamos.LogicaServicio.Entidades;

namespace Prestamos.LogicaServicio.Datos;

/// <summary>
/// Capa de datos (Unidad 5): única clase que sabe cómo y dónde se guardan
/// los datos. Lee y escribe la lista completa en un archivo JSON.
/// </summary>
public class ProductoRepositorio
{
    private readonly string _rutaArchivo;

    // La ruta la recibe por constructor: la decide el Service, no el repositorio.
    public ProductoRepositorio(string rutaArchivo)
    {
        _rutaArchivo = rutaArchivo;
    }

    /// <summary>
    /// Devuelve la lista guardada. Si el archivo todavía no existe
    /// (primera ejecución) devuelve una lista vacía en lugar de fallar.
    /// </summary>
    public List<Producto> ObtenerTodos()
    {
        try
        {
            if (!File.Exists(_rutaArchivo))
            {
                return new List<Producto>();
            }

            string json = File.ReadAllText(_rutaArchivo);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Producto>();
            }

            return JsonConvert.DeserializeObject<List<Producto>>(json)
                ?? new List<Producto>();
        }
        catch (JsonException)
        {
            // El archivo existe pero no es un JSON válido.
            return new List<Producto>();
        }
    }

    /// <summary>Serializa la lista completa y sobreescribe el archivo.</summary>
    public void Guardar(List<Producto> productos)
    {
        string json = JsonConvert.SerializeObject(productos, Formatting.Indented);
        File.WriteAllText(_rutaArchivo, json);
    }
}
