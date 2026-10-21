using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Prestamos.LogicaServicio.Entidades;
using Prestamos.LogicaServicio.Servicios;
using Prestamos.LogicaServicio.Datos;

namespace Prestamos.LogicaTest;

/// <summary>
/// Tests del servicio (Unidad 4). Patrón Arrange-Act-Assert y nomenclatura
/// Metodo_Escenario_ResultadoEsperado.
/// </summary>
[TestFixture]
public class ProductoServiceTest
{
    private string _rutaArchivo = string.Empty;
    private ProductoService _servicio = null!;

    [SetUp]
    public void Preparar()
    {
        // Cada test arranca con un archivo propio y vacío: los tests quedan
        // aislados entre sí y no tocan los datos reales de la aplicación.
        _rutaArchivo = Path.Combine(
            Path.GetTempPath(),
            $"productos-test-{Guid.NewGuid():N}.json");

        _servicio = new ProductoService(new ProductoRepositorio(_rutaArchivo));
    }

    [TearDown]
    public void Limpiar()
    {
        if (File.Exists(_rutaArchivo))
        {
            File.Delete(_rutaArchivo);
        }
    }

    [Test]
    public void Agregar_ProductoValido_AsignaIdUnoYLoGuarda()
    {
        // Arrange
        var producto = new Producto { Nombre = "Ejemplo", Precio = 100m };

        // Act
        Producto agregado = _servicio.Agregar(producto);

        // Assert
        Assert.That(agregado.Id, Is.EqualTo(1));
        Assert.That(_servicio.ObtenerTodos(), Has.Count.EqualTo(1));
    }

    [Test]
    public void Agregar_NombreVacio_LanzaArgumentException()
    {
        // Arrange
        var producto = new Producto { Nombre = "", Precio = 100m };

        // Act + Assert
        Assert.Throws<ArgumentException>(() => _servicio.Agregar(producto));
    }

    [Test]
    public void ObtenerPorId_IdInexistente_DevuelveNull()
    {
        // Act
        Producto? encontrado = _servicio.ObtenerPorId(99);

        // Assert
        Assert.That(encontrado, Is.Null);
    }

    [Test]
    public void Eliminar_IdExistente_DevuelveTrueYLoSaca()
    {
        // Arrange
        Producto agregado = _servicio.Agregar(
            new Producto { Nombre = "Para borrar", Precio = 50m });

        // Act
        bool eliminado = _servicio.Eliminar(agregado.Id);

        // Assert
        Assert.That(eliminado, Is.True);
        Assert.That(_servicio.ObtenerTodos(), Is.Empty);
    }

    [Test]
    public void ObtenerTodos_DespuesDeGuardar_LeeLoPersistido()
    {
        // Arrange: se agrega con un servicio y se lee con otro, apuntando
        // al mismo archivo: así se verifica que los datos realmente persisten.
        _servicio.Agregar(new Producto { Nombre = "Persistido", Precio = 10m });

        var otroServicio = new ProductoService(new ProductoRepositorio(_rutaArchivo));

        // Act
        List<Producto> productos = otroServicio.ObtenerTodos();

        // Assert
        Assert.That(productos, Has.Count.EqualTo(1));
        Assert.That(productos[0].Nombre, Is.EqualTo("Persistido"));
    }

    // TODO: agregar los tests de las reglas de negocio de tu enunciado,
    // incluyendo los casos que tienen que ser rechazados.
}
