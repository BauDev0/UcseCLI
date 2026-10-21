namespace Prestamos.Api.DTOs;

/// <summary>
/// Response: lo que la API le devuelve al cliente. Permite exponer solo
/// los datos que el cliente necesita, sin publicar la entidad completa.
/// </summary>
public class ProductoResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public decimal Precio { get; set; }
}
