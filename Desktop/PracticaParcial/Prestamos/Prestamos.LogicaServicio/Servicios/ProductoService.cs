using System;
using System.Collections.Generic;
using System.Linq;
using Prestamos.LogicaServicio.Entidades;
using Prestamos.LogicaServicio.Datos;

namespace Prestamos.LogicaServicio.Servicios;

/// <summary>
/// Capa de lógica de negocio (Unidades 2 y 5). Acá viven las reglas.
/// No escribe por consola ni lee input: recibe parámetros, devuelve objetos
/// o lanza excepciones. Así la misma lógica sirve para una consola o una Web API.
/// </summary>
public class ProductoService
{
    private readonly ProductoRepositorio _repositorio;

    // La ruta del archivo la decide el Service (Unidad 5).
    public ProductoService()
        : this(new ProductoRepositorio("productos.json"))
    {
    }

    // Constructor para inyectar el repositorio (Unidad 6): lo usan los tests
    // para trabajar sobre un archivo temporal en lugar del real.
    public ProductoService(ProductoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public List<Producto> ObtenerTodos()
    {
        return _repositorio.ObtenerTodos();
    }

    /// <summary>Devuelve null si no existe: buscar algo que no está no es un error.</summary>
    public Producto? ObtenerPorId(int id)
    {
        return _repositorio.ObtenerTodos().FirstOrDefault(producto => producto.Id == id);
    }

    /// <summary>Agrega una entidad nueva asignándole el Id siguiente.</summary>
    public Producto Agregar(Producto producto)
    {
        if (producto == null)
        {
            throw new ArgumentNullException(nameof(producto));
        }

        // TODO: reemplazar por las validaciones que pide tu enunciado.
        if (string.IsNullOrWhiteSpace(producto.Nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.");
        }

        // Patrón de la Unidad 5: leer todo, modificar en memoria, guardar todo.
        List<Producto> productos = _repositorio.ObtenerTodos();

        producto.Id = productos.Count > 0 ? productos.Max(x => x.Id) + 1 : 1;
        productos.Add(producto);

        _repositorio.Guardar(productos);

        return producto;
    }

    /// <summary>Devuelve false si no existe la entidad con ese Id.</summary>
    public bool Actualizar(int id, Producto productoActualizado)
    {
        List<Producto> productos = _repositorio.ObtenerTodos();
        Producto? existente = productos.FirstOrDefault(x => x.Id == id);

        if (existente == null)
        {
            return false;
        }

        existente.Nombre = productoActualizado.Nombre;
        existente.Precio = productoActualizado.Precio;
        existente.Activo = productoActualizado.Activo;

        _repositorio.Guardar(productos);

        return true;
    }

    /// <summary>Devuelve false si no existe la entidad con ese Id.</summary>
    public bool Eliminar(int id)
    {
        List<Producto> productos = _repositorio.ObtenerTodos();
        Producto? existente = productos.FirstOrDefault(x => x.Id == id);

        if (existente == null)
        {
            return false;
        }

        productos.Remove(existente);
        _repositorio.Guardar(productos);

        return true;
    }

    /// <summary>Ejemplo de búsqueda con LINQ (Unidad 2 - sección 10).</summary>
    public List<Producto> BuscarPorNombre(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return _repositorio.ObtenerTodos();
        }

        return _repositorio.ObtenerTodos()
            .Where(x => x.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
