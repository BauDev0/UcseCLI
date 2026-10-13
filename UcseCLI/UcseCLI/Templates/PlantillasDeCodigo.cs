namespace UcseCLI.Templates;

/// <summary>
/// Plantillas de archivos .cs. Son ejemplos alineados con los apuntes de la
/// materia (Unidades 2, 4, 5 y 6) pensados para adaptarse, no para dejarse tal cual.
///
/// Marcadores:
///   __NS__            namespace del archivo
///   __USINGS__        bloque de using (puede quedar vacío)
///   __NOMBRE__        nombre de la clase / interfaz / enum
///   __ENTIDAD__       entidad de ejemplo en PascalCase  (Producto)
///   __entidad__       la misma en camelCase             (producto)
///   __entidades__     la misma en plural                (productos)
/// </summary>
public static class PlantillasDeCodigo
{
    public const string Clase =
        """
        __NS__/// <summary>
        /// TODO: describir qué representa esta clase.
        /// </summary>
        public class __NOMBRE__
        {
        }
        """;

    public const string Interfaz =
        """
        __NS__/// <summary>
        /// Contrato que deben cumplir las clases que la implementen (Unidad 2 - interfaces).
        /// </summary>
        public interface __NOMBRE__
        {
            // Las interfaces declaran la firma, sin implementación ni modificador de acceso.
            // TODO: declarar los métodos que tienen que cumplir las implementaciones.
            // void Ejemplo(string parametro);
        }
        """;

    public const string ClaseAbstracta =
        """
        __NS__/// <summary>
        /// Clase abstracta de ejemplo (Unidad 2 - clases abstractas y polimorfismo).
        /// No se puede instanciar: define comportamiento común y obliga a las clases
        /// derivadas a implementar los miembros abstractos.
        /// </summary>
        public abstract class __NOMBRE__
        {
            // Miembro abstracto: cada clase derivada lo implementa con 'override'.
            // TODO: reemplazar por lo que necesite tu jerarquía.
            public abstract string Describir();

            // Miembro concreto: lo heredan todas las derivadas tal cual.
            public override string ToString() => Describir();
        }
        """;

    public const string Enumerador =
        """
        __NS__/// <summary>
        /// Enumerador de ejemplo (Unidad 2 - constantes y enumeradores).
        /// Sirve para representar un conjunto cerrado de valores posibles.
        /// </summary>
        public enum __NOMBRE__
        {
            // TODO: reemplazar por los valores de tu dominio.
            Pendiente,
            EnProceso,
            Finalizado
        }
        """;

    public const string Entidad =
        """
        __NS__/// <summary>
        /// Entidad de ejemplo (Unidad 2). Representa un objeto del dominio:
        /// solo datos y comportamiento propio, sin consola ni acceso a archivos.
        /// </summary>
        public class __ENTIDAD__
        {
            public int Id { get; set; }

            public string Nombre { get; set; } = string.Empty;

            // TODO: reemplazar estas propiedades por las que pide tu enunciado.
            public decimal Precio { get; set; }

            public bool Activo { get; set; } = true;

            public override string ToString() => $"#{Id} - {Nombre}";
        }
        """;

    public const string Repositorio =
        """
        using System.Collections.Generic;
        using System.IO;
        using Newtonsoft.Json;
        __USING_ENTIDADES__
        __NS__/// <summary>
        /// Capa de datos (Unidad 5): única clase que sabe cómo y dónde se guardan
        /// los datos. Lee y escribe la lista completa en un archivo JSON.
        /// </summary>
        public class __ENTIDAD__Repositorio
        {
            private readonly string _rutaArchivo;

            // La ruta la recibe por constructor: la decide el Service, no el repositorio.
            public __ENTIDAD__Repositorio(string rutaArchivo)
            {
                _rutaArchivo = rutaArchivo;
            }

            /// <summary>
            /// Devuelve la lista guardada. Si el archivo todavía no existe
            /// (primera ejecución) devuelve una lista vacía en lugar de fallar.
            /// </summary>
            public List<__ENTIDAD__> ObtenerTodos()
            {
                try
                {
                    if (!File.Exists(_rutaArchivo))
                    {
                        return new List<__ENTIDAD__>();
                    }

                    string json = File.ReadAllText(_rutaArchivo);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<__ENTIDAD__>();
                    }

                    return JsonConvert.DeserializeObject<List<__ENTIDAD__>>(json)
                        ?? new List<__ENTIDAD__>();
                }
                catch (JsonException)
                {
                    // El archivo existe pero no es un JSON válido.
                    return new List<__ENTIDAD__>();
                }
            }

            /// <summary>Serializa la lista completa y sobreescribe el archivo.</summary>
            public void Guardar(List<__ENTIDAD__> __entidades__)
            {
                string json = JsonConvert.SerializeObject(__entidades__, Formatting.Indented);
                File.WriteAllText(_rutaArchivo, json);
            }
        }
        """;

    public const string Servicio =
        """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        __USING_ENTIDADES____USING_DATOS__
        __NS__/// <summary>
        /// Capa de lógica de negocio (Unidades 2 y 5). Acá viven las reglas.
        /// No escribe por consola ni lee input: recibe parámetros, devuelve objetos
        /// o lanza excepciones. Así la misma lógica sirve para una consola o una Web API.
        /// </summary>
        public class __ENTIDAD__Service
        {
            private readonly __ENTIDAD__Repositorio _repositorio;

            // La ruta del archivo la decide el Service (Unidad 5).
            public __ENTIDAD__Service()
                : this(new __ENTIDAD__Repositorio("__entidades__.json"))
            {
            }

            // Constructor para inyectar el repositorio (Unidad 6): lo usan los tests
            // para trabajar sobre un archivo temporal en lugar del real.
            public __ENTIDAD__Service(__ENTIDAD__Repositorio repositorio)
            {
                _repositorio = repositorio;
            }

            public List<__ENTIDAD__> ObtenerTodos()
            {
                return _repositorio.ObtenerTodos();
            }

            /// <summary>Devuelve null si no existe: buscar algo que no está no es un error.</summary>
            public __ENTIDAD__? ObtenerPorId(int id)
            {
                return _repositorio.ObtenerTodos().FirstOrDefault(__entidad__ => __entidad__.Id == id);
            }

            /// <summary>Agrega una entidad nueva asignándole el Id siguiente.</summary>
            public __ENTIDAD__ Agregar(__ENTIDAD__ __entidad__)
            {
                if (__entidad__ == null)
                {
                    throw new ArgumentNullException(nameof(__entidad__));
                }

                // TODO: reemplazar por las validaciones que pide tu enunciado.
                if (string.IsNullOrWhiteSpace(__entidad__.Nombre))
                {
                    throw new ArgumentException("El nombre es obligatorio.");
                }

                // Patrón de la Unidad 5: leer todo, modificar en memoria, guardar todo.
                List<__ENTIDAD__> __entidades__ = _repositorio.ObtenerTodos();

                __entidad__.Id = __entidades__.Count > 0 ? __entidades__.Max(x => x.Id) + 1 : 1;
                __entidades__.Add(__entidad__);

                _repositorio.Guardar(__entidades__);

                return __entidad__;
            }

            /// <summary>Devuelve false si no existe la entidad con ese Id.</summary>
            public bool Actualizar(int id, __ENTIDAD__ __entidad__Actualizado)
            {
                List<__ENTIDAD__> __entidades__ = _repositorio.ObtenerTodos();
                __ENTIDAD__? existente = __entidades__.FirstOrDefault(x => x.Id == id);

                if (existente == null)
                {
                    return false;
                }

                existente.Nombre = __entidad__Actualizado.Nombre;
                existente.Precio = __entidad__Actualizado.Precio;
                existente.Activo = __entidad__Actualizado.Activo;

                _repositorio.Guardar(__entidades__);

                return true;
            }

            /// <summary>Devuelve false si no existe la entidad con ese Id.</summary>
            public bool Eliminar(int id)
            {
                List<__ENTIDAD__> __entidades__ = _repositorio.ObtenerTodos();
                __ENTIDAD__? existente = __entidades__.FirstOrDefault(x => x.Id == id);

                if (existente == null)
                {
                    return false;
                }

                __entidades__.Remove(existente);
                _repositorio.Guardar(__entidades__);

                return true;
            }

            /// <summary>Ejemplo de búsqueda con LINQ (Unidad 2 - sección 10).</summary>
            public List<__ENTIDAD__> BuscarPorNombre(string texto)
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
        """;

    public const string TestDeServicio =
        """
        using System;
        using System.Collections.Generic;
        using System.IO;
        using NUnit.Framework;
        __USING_ENTIDADES____USING_SERVICIOS____USING_DATOS__
        __NS__/// <summary>
        /// Tests del servicio (Unidad 4). Patrón Arrange-Act-Assert y nomenclatura
        /// Metodo_Escenario_ResultadoEsperado.
        /// </summary>
        [TestFixture]
        public class __ENTIDAD__ServiceTest
        {
            private string _rutaArchivo = string.Empty;
            private __ENTIDAD__Service _servicio = null!;

            [SetUp]
            public void Preparar()
            {
                // Cada test arranca con un archivo propio y vacío: los tests quedan
                // aislados entre sí y no tocan los datos reales de la aplicación.
                _rutaArchivo = Path.Combine(
                    Path.GetTempPath(),
                    $"__entidades__-test-{Guid.NewGuid():N}.json");

                _servicio = new __ENTIDAD__Service(new __ENTIDAD__Repositorio(_rutaArchivo));
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
            public void Agregar___ENTIDAD__Valido_AsignaIdUnoYLoGuarda()
            {
                // Arrange
                var __entidad__ = new __ENTIDAD__ { Nombre = "Ejemplo", Precio = 100m };

                // Act
                __ENTIDAD__ agregado = _servicio.Agregar(__entidad__);

                // Assert
                Assert.That(agregado.Id, Is.EqualTo(1));
                Assert.That(_servicio.ObtenerTodos(), Has.Count.EqualTo(1));
            }

            [Test]
            public void Agregar_NombreVacio_LanzaArgumentException()
            {
                // Arrange
                var __entidad__ = new __ENTIDAD__ { Nombre = "", Precio = 100m };

                // Act + Assert
                Assert.Throws<ArgumentException>(() => _servicio.Agregar(__entidad__));
            }

            [Test]
            public void ObtenerPorId_IdInexistente_DevuelveNull()
            {
                // Act
                __ENTIDAD__? encontrado = _servicio.ObtenerPorId(99);

                // Assert
                Assert.That(encontrado, Is.Null);
            }

            [Test]
            public void Eliminar_IdExistente_DevuelveTrueYLoSaca()
            {
                // Arrange
                __ENTIDAD__ agregado = _servicio.Agregar(
                    new __ENTIDAD__ { Nombre = "Para borrar", Precio = 50m });

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
                _servicio.Agregar(new __ENTIDAD__ { Nombre = "Persistido", Precio = 10m });

                var otroServicio = new __ENTIDAD__Service(new __ENTIDAD__Repositorio(_rutaArchivo));

                // Act
                List<__ENTIDAD__> __entidades__ = otroServicio.ObtenerTodos();

                // Assert
                Assert.That(__entidades__, Has.Count.EqualTo(1));
                Assert.That(__entidades__[0].Nombre, Is.EqualTo("Persistido"));
            }

            // TODO: agregar los tests de las reglas de negocio de tu enunciado,
            // incluyendo los casos que tienen que ser rechazados.
        }
        """;

    public const string TestDeClase =
        """
        using System;
        using NUnit.Framework;
        __USINGS__
        __NS__/// <summary>
        /// Tests de __NOMBRE__ (Unidad 4): patrón Arrange-Act-Assert y nomenclatura
        /// Metodo_Escenario_ResultadoEsperado.
        /// </summary>
        [TestFixture]
        public class __NOMBRE__Test
        {
            [Test]
            public void Metodo_Escenario_ResultadoEsperado()
            {
                // Arrange: preparar las condiciones iniciales.
                // var sut = new __NOMBRE__();

                // Act: ejecutar el método que se prueba.
                // var resultado = sut.Metodo();

                // Assert: verificar el resultado esperado.
                Assert.That(true, Is.True); // TODO: reemplazar por la verificación real.
            }
        }
        """;
}
