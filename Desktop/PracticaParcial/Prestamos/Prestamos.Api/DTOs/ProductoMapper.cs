using Prestamos.LogicaServicio.Entidades;

namespace Prestamos.Api.DTOs;

/// <summary>
/// Mapeo entre DTOs y entidad, concentrado en un solo lugar (Unidad 6 - sección 9).
/// Son métodos de extensión: se llaman como producto.AProductoResponse().
/// </summary>
public static class ProductoMapper
{
    // Modelo -> Response
    public static ProductoResponse AProductoResponse(this Producto producto)
    {
        return new ProductoResponse
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Precio = producto.Precio
        };
    }

    // Request -> Modelo nuevo
    public static Producto AProducto(this ProductoRequest request)
    {
        return new Producto
        {
            Nombre = request.Nombre,
            Precio = request.Precio,
            Activo = true
        };
    }
}
