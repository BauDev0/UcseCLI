namespace UcseCLI.Templates;

/// <summary>Fragmento de código de referencia listo para copiar y adaptar.</summary>
public sealed record Ejemplo(string Clave, string Titulo, string Unidad, string Codigo);

/// <summary>
/// Catálogo de fragmentos reutilizables, alineados con los apuntes de la materia.
/// Se consultan con "ucse ejemplos" y se pueden guardar con "--guardar".
/// </summary>
public static class Ejemplos
{
    public static IReadOnlyList<Ejemplo> Todos { get; } = new List<Ejemplo>
    {
        new("herencia", "Herencia y polimorfismo", "Unidad 2",
            """
            // Clase base: lo común a todas las figuras.
            public abstract class Figura
            {
                public abstract double CalcularArea();

                // Método concreto: se hereda tal cual.
                public string Describir() => $"{GetType().Name} con área {CalcularArea():F2}";
            }

            public class Circulo : Figura
            {
                public double Radio { get; }

                public Circulo(double radio)
                {
                    if (radio <= 0)
                        throw new ArgumentException("El radio debe ser mayor a cero.");

                    Radio = radio;
                }

                public override double CalcularArea() => Math.PI * Radio * Radio;
            }

            public class Rectangulo : Figura
            {
                public double Ancho { get; }
                public double Alto { get; }

                public Rectangulo(double ancho, double alto)
                {
                    Ancho = ancho;
                    Alto = alto;
                }

                public override double CalcularArea() => Ancho * Alto;
            }

            // Polimorfismo: la lista es de Figura y cada objeto responde a su manera.
            List<Figura> figuras = new() { new Circulo(5), new Rectangulo(4, 6) };

            foreach (Figura figura in figuras)
            {
                Console.WriteLine(figura.Describir());
            }
            """),

        new("interfaz", "Interfaz e implementación", "Unidad 2",
            """
            // El contrato: qué se puede hacer, sin decir cómo.
            public interface INotificador
            {
                void Notificar(string mensaje);
            }

            public class NotificadorConsola : INotificador
            {
                public void Notificar(string mensaje) => Console.WriteLine($"[consola] {mensaje}");
            }

            public class NotificadorArchivo : INotificador
            {
                private readonly string _ruta;

                public NotificadorArchivo(string ruta) => _ruta = ruta;

                public void Notificar(string mensaje) =>
                    File.AppendAllText(_ruta, mensaje + Environment.NewLine);
            }

            // Quien usa la interfaz no depende de la implementación concreta.
            public class PedidoService
            {
                private readonly INotificador _notificador;

                public PedidoService(INotificador notificador) => _notificador = notificador;

                public void Confirmar(int id) => _notificador.Notificar($"Pedido {id} confirmado");
            }
            """),

        new("excepciones", "Excepciones y validaciones", "Unidad 2",
            """
            public class CuentaBancaria
            {
                public decimal Saldo { get; private set; }

                public void Depositar(decimal monto)
                {
                    // Datos inválidos: ArgumentException.
                    if (monto <= 0)
                        throw new ArgumentException("El monto debe ser mayor a cero.", nameof(monto));

                    Saldo += monto;
                }

                public void Extraer(decimal monto)
                {
                    // Operación inválida para el estado actual: InvalidOperationException.
                    if (monto > Saldo)
                        throw new InvalidOperationException("Saldo insuficiente.");

                    Saldo -= monto;
                }
            }

            // Quien llama decide cómo informar el error.
            try
            {
                cuenta.Extraer(5000m);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"No se pudo extraer: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Saldo final: {cuenta.Saldo}");
            }
            """),

        new("linq", "LINQ y expresiones lambda", "Unidad 2",
            """
            List<Producto> productos = servicio.ObtenerTodos();

            // Filtrar
            List<Producto> baratos = productos
                .Where(p => p.Precio < 100)
                .ToList();

            // Ordenar y proyectar
            List<string> nombres = productos
                .OrderBy(p => p.Nombre)
                .Select(p => p.Nombre)
                .ToList();

            // Buscar uno (null si no hay coincidencia)
            Producto? primero = productos.FirstOrDefault(p => p.Categoria == "Electrónica");

            // Agregados
            int cantidad = productos.Count(p => p.Activo);
            decimal total = productos.Sum(p => p.Precio);
            decimal promedio = productos.Any() ? productos.Average(p => p.Precio) : 0;

            // Agrupar
            var porCategoria = productos
                .GroupBy(p => p.Categoria)
                .Select(grupo => new { Categoria = grupo.Key, Cantidad = grupo.Count() });
            """),

        new("json", "Persistencia en archivo JSON", "Unidad 5",
            """
            // Requiere el paquete Newtonsoft.Json:
            //   dotnet add <Proyecto>/<Proyecto>.csproj package Newtonsoft.Json
            using Newtonsoft.Json;

            public class ProductoRepositorio
            {
                private readonly string _rutaArchivo;

                public ProductoRepositorio(string rutaArchivo) => _rutaArchivo = rutaArchivo;

                public List<Producto> ObtenerTodos()
                {
                    // Primera ejecución: el archivo todavía no existe.
                    if (!File.Exists(_rutaArchivo))
                        return new List<Producto>();

                    string json = File.ReadAllText(_rutaArchivo);

                    if (string.IsNullOrWhiteSpace(json))
                        return new List<Producto>();

                    return JsonConvert.DeserializeObject<List<Producto>>(json)
                        ?? new List<Producto>();
                }

                public void Guardar(List<Producto> productos)
                {
                    string json = JsonConvert.SerializeObject(productos, Formatting.Indented);
                    File.WriteAllText(_rutaArchivo, json);
                }
            }

            // Patrón: leer todo -> modificar en memoria -> guardar todo.
            List<Producto> productos = repositorio.ObtenerTodos();
            productos.Add(nuevo);
            repositorio.Guardar(productos);
            """),

        new("nunit", "Tests con NUnit (Arrange-Act-Assert)", "Unidad 4",
            """
            using NUnit.Framework;

            [TestFixture]
            public class CalculadoraTest
            {
                private Calculadora _calculadora = null!;

                [SetUp]
                public void Preparar() => _calculadora = new Calculadora();

                [Test]
                public void Sumar_DosNumerosPositivos_RetornaLaSuma()
                {
                    // Arrange
                    int primerNumero = 10;
                    int segundoNumero = 5;

                    // Act
                    int resultado = _calculadora.Sumar(primerNumero, segundoNumero);

                    // Assert
                    Assert.That(resultado, Is.EqualTo(15));
                }

                [Test]
                public void Dividir_DivisorCero_LanzaDivideByZeroException()
                {
                    Assert.Throws<DivideByZeroException>(() => _calculadora.Dividir(10, 0));
                }

                // Varios casos con el mismo cuerpo de test.
                [TestCase(2, 3, 5)]
                [TestCase(-1, 1, 0)]
                [TestCase(0, 0, 0)]
                public void Sumar_DistintosValores_RetornaLaSuma(int a, int b, int esperado)
                {
                    Assert.That(_calculadora.Sumar(a, b), Is.EqualTo(esperado));
                }

                // Comparación de decimales: siempre con tolerancia.
                [Test]
                public void CalcularArea_RadioValido_RetornaArea()
                {
                    Assert.That(new Circulo(5).CalcularArea(), Is.EqualTo(78.54).Within(0.01));
                }
            }
            """),

        new("controller", "Controller REST con DTOs", "Unidad 6",
            """
            [ApiController]
            [Route("api/[controller]")]
            public class ProductoController : ControllerBase
            {
                private readonly ProductoService _servicio;

                // El servicio llega por inyección de dependencias (registrado en Program.cs).
                public ProductoController(ProductoService servicio) => _servicio = servicio;

                [HttpGet]
                public IActionResult ObtenerTodos() =>
                    Ok(_servicio.ObtenerTodos().Select(p => p.AProductoResponse()));

                [HttpGet("{id}")]
                public IActionResult ObtenerPorId(int id)
                {
                    Producto? producto = _servicio.ObtenerPorId(id);

                    if (producto == null)
                        return NotFound();            // 404

                    return Ok(producto.AProductoResponse());   // 200
                }

                [HttpPost]
                public IActionResult Crear([FromBody] ProductoRequest request)
                {
                    Producto producto = request.AProducto();
                    _servicio.Agregar(producto);

                    return StatusCode(201, producto.AProductoResponse());   // 201
                }

                [HttpDelete("{id}")]
                public IActionResult Eliminar(int id) =>
                    _servicio.Eliminar(id) ? NoContent() : NotFound();      // 204 / 404
            }

            // Filtrar por query string: GET api/producto/buscar?categoria=Electrónica
            [HttpGet("buscar")]
            public IActionResult Buscar([FromQuery] string categoria) =>
                Ok(_servicio.BuscarPorCategoria(categoria).Select(p => p.AProductoResponse()));
            """),

        new("fetch", "Consumir la API desde JavaScript", "Unidad 7",
            """
            const API_URL = "http://localhost:5000/api/producto";

            // GET
            async function listar() {
                const respuesta = await fetch(API_URL);

                if (!respuesta.ok) {
                    throw new Error(`La API respondió ${respuesta.status}`);
                }

                return await respuesta.json();
            }

            // POST
            async function crear(producto) {
                const respuesta = await fetch(API_URL, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(producto)
                });

                if (!respuesta.ok) {
                    throw new Error(`La API respondió ${respuesta.status}`);
                }

                return await respuesta.json();
            }

            // DELETE
            async function eliminar(id) {
                await fetch(`${API_URL}/${id}`, { method: "DELETE" });
            }

            // Dibujar el resultado en el DOM sin armar HTML a mano.
            function dibujar(productos) {
                const tbody = document.getElementById("cuerpo-tabla");
                tbody.innerHTML = "";

                for (const producto of productos) {
                    const fila = document.createElement("tr");
                    const celda = document.createElement("td");

                    celda.textContent = producto.nombre;
                    fila.appendChild(celda);
                    tbody.appendChild(fila);
                }
            }
            """),

        new("git", "Flujo de trabajo con Git", "Unidad 3",
            """
            # Empezar un repositorio y publicarlo
            git init
            git add .
            git commit -m "Estructura inicial de la solución"
            git branch -M main
            git remote add origin https://github.com/usuario/repositorio.git
            git push -u origin main

            # Trabajar en una funcionalidad sin romper main
            git switch -c feature/alta-de-productos
            git add .
            git commit -m "Agrega alta de productos con validaciones"
            git push -u origin feature/alta-de-productos
            # ... y desde GitHub se abre el Pull Request hacia main

            # Traer los cambios de los demás antes de seguir
            git switch main
            git pull origin main
            git switch feature/alta-de-productos
            git merge main

            # Ver en qué estado está todo
            git status
            git log --oneline --graph --all
            """)
    };

    public static Ejemplo? Buscar(string clave) =>
        Todos.FirstOrDefault(ejemplo =>
            string.Equals(ejemplo.Clave, clave, StringComparison.OrdinalIgnoreCase));
}
