using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Prestamos.Api.DTOs;
using Prestamos.LogicaServicio.Entidades;
using Prestamos.LogicaServicio.Servicios;

namespace Prestamos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductoController : ControllerBase
{
    private readonly ProductoService _servicio;

    // El servicio llega por inyección de dependencias: está registrado en Program.cs.
    public ProductoController(ProductoService servicio)
    {
        _servicio = servicio;
    }

    // GET api/producto
    [HttpGet]
    public IActionResult ObtenerTodos()
    {
        IEnumerable<ProductoResponse> respuesta = _servicio
            .ObtenerTodos()
            .Select(producto => producto.AProductoResponse());

        return Ok(respuesta);
    }

    // GET api/producto/5
    [HttpGet("{id}")]
    public IActionResult ObtenerPorId(int id)
    {
        Producto? producto = _servicio.ObtenerPorId(id);

        if (producto == null)
        {
            return NotFound();
        }

        return Ok(producto.AProductoResponse());
    }

    // POST api/producto
    [HttpPost]
    public IActionResult Crear([FromBody] ProductoRequest request)
    {
        // El Controller mapea Request -> Modelo; el Service solo conoce el Modelo.
        Producto producto = request.AProducto();

        _servicio.Agregar(producto);

        return StatusCode(201, producto.AProductoResponse());
    }

    // PUT api/producto/5
    [HttpPut("{id}")]
    public IActionResult Actualizar(int id, [FromBody] ProductoRequest request)
    {
        Producto producto = request.AProducto();
        producto.Id = id; // el Id viene de la URL, no del Request

        if (!_servicio.Actualizar(id, producto))
        {
            return NotFound();
        }

        return Ok(producto.AProductoResponse());
    }

    // DELETE api/producto/5
    [HttpDelete("{id}")]
    public IActionResult Eliminar(int id)
    {
        if (!_servicio.Eliminar(id))
        {
            return NotFound();
        }

        return NoContent();
    }
}
