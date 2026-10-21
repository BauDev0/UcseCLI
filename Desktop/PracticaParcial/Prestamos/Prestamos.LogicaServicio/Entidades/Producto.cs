namespace Prestamos.LogicaServicio.Entidades;

/// <summary>
/// Entidad de ejemplo (Unidad 2). Representa un objeto del dominio:
/// solo datos y comportamiento propio, sin consola ni acceso a archivos.
/// </summary>
public class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    // TODO: reemplazar estas propiedades por las que pide tu enunciado.
    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;

    public override string ToString() => $"#{Id} - {Nombre}";
}
