namespace UcseCLI.Templates;

/// <summary>
/// Plantillas del proyecto Web API (Unidad 6): Program.cs, controller, DTOs y
/// mapper. El Controller recibe/devuelve DTOs y mapea antes de llamar al Service;
/// el Service nunca conoce los DTOs.
/// </summary>
public static class PlantillasWebApi
{
    /// <summary>
    /// Program.cs cuando la plantilla de dotnet trae Swashbuckle (SDK 8):
    /// queda Swagger UI navegable en /swagger.
    /// </summary>
    public const string ProgramaConSwagger =
        """
        __USING_SERVICIOS__
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // CORS: lo necesita el frontend (Unidad 7) para consumir la API desde el navegador.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirTodo", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        __REGISTRO_SERVICIOS__

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("PermitirTodo");
        app.MapControllers();

        app.Run();
        """;

    /// <summary>
    /// Program.cs cuando la plantilla de dotnet trae Microsoft.AspNetCore.OpenApi
    /// en lugar de Swashbuckle (SDK 9 o posterior): expone /openapi/v1.json.
    /// </summary>
    public const string ProgramaConOpenApi =
        """
        __USING_SERVICIOS__
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // CORS: lo necesita el frontend (Unidad 7) para consumir la API desde el navegador.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirTodo", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        __REGISTRO_SERVICIOS__

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            // Documento OpenAPI en /openapi/v1.json
            app.MapOpenApi();
        }

        app.UseCors("PermitirTodo");
        app.MapControllers();

        app.Run();
        """;

    /// <summary>
    /// Program.cs cuando la plantilla no trae ningún paquete de OpenAPI:
    /// la API queda con controllers y CORS, sin documentación automática.
    /// </summary>
    public const string ProgramaSinOpenApi =
        """
        __USING_SERVICIOS__
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // CORS: lo necesita el frontend (Unidad 7) para consumir la API desde el navegador.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirTodo", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        __REGISTRO_SERVICIOS__

        var app = builder.Build();

        app.UseCors("PermitirTodo");
        app.MapControllers();

        app.Run();
        """;

    public const string Controlador =
        """
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.AspNetCore.Mvc;
        __USING_DTOS____USING_ENTIDADES____USING_SERVICIOS__
        __NS__[ApiController]
        [Route("api/[controller]")]
        public class __ENTIDAD__Controller : ControllerBase
        {
            private readonly __ENTIDAD__Service _servicio;

            // El servicio llega por inyección de dependencias: está registrado en Program.cs.
            public __ENTIDAD__Controller(__ENTIDAD__Service servicio)
            {
                _servicio = servicio;
            }

            // GET api/__entidad__
            [HttpGet]
            public IActionResult ObtenerTodos()
            {
                IEnumerable<__ENTIDAD__Response> respuesta = _servicio
                    .ObtenerTodos()
                    .Select(__entidad__ => __entidad__.A__ENTIDAD__Response());

                return Ok(respuesta);
            }

            // GET api/__entidad__/5
            [HttpGet("{id}")]
            public IActionResult ObtenerPorId(int id)
            {
                __ENTIDAD__? __entidad__ = _servicio.ObtenerPorId(id);

                if (__entidad__ == null)
                {
                    return NotFound();
                }

                return Ok(__entidad__.A__ENTIDAD__Response());
            }

            // POST api/__entidad__
            [HttpPost]
            public IActionResult Crear([FromBody] __ENTIDAD__Request request)
            {
                // El Controller mapea Request -> Modelo; el Service solo conoce el Modelo.
                __ENTIDAD__ __entidad__ = request.A__ENTIDAD__();

                _servicio.Agregar(__entidad__);

                return StatusCode(201, __entidad__.A__ENTIDAD__Response());
            }

            // PUT api/__entidad__/5
            [HttpPut("{id}")]
            public IActionResult Actualizar(int id, [FromBody] __ENTIDAD__Request request)
            {
                __ENTIDAD__ __entidad__ = request.A__ENTIDAD__();
                __entidad__.Id = id; // el Id viene de la URL, no del Request

                if (!_servicio.Actualizar(id, __entidad__))
                {
                    return NotFound();
                }

                return Ok(__entidad__.A__ENTIDAD__Response());
            }

            // DELETE api/__entidad__/5
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
        """;

    public const string DtoRequest =
        """
        using System.ComponentModel.DataAnnotations;

        __NS__/// <summary>
        /// Request: lo que manda el cliente en el body de un POST o PUT.
        /// No incluye el Id (en POST lo genera el servidor, en PUT viene en la URL).
        /// Las validaciones van acá, no en la entidad (Unidad 6 - sección 10).
        /// </summary>
        public class __ENTIDAD__Request
        {
            [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
            public string Nombre { get; set; } = string.Empty;

            [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor a 0")]
            public decimal Precio { get; set; }
        }
        """;

    public const string DtoResponse =
        """
        __NS__/// <summary>
        /// Response: lo que la API le devuelve al cliente. Permite exponer solo
        /// los datos que el cliente necesita, sin publicar la entidad completa.
        /// </summary>
        public class __ENTIDAD__Response
        {
            public int Id { get; set; }

            public string Nombre { get; set; } = string.Empty;

            public decimal Precio { get; set; }
        }
        """;

    public const string Mapper =
        """
        __USING_ENTIDADES__
        __NS__/// <summary>
        /// Mapeo entre DTOs y entidad, concentrado en un solo lugar (Unidad 6 - sección 9).
        /// Son métodos de extensión: se llaman como __entidad__.A__ENTIDAD__Response().
        /// </summary>
        public static class __ENTIDAD__Mapper
        {
            // Modelo -> Response
            public static __ENTIDAD__Response A__ENTIDAD__Response(this __ENTIDAD__ __entidad__)
            {
                return new __ENTIDAD__Response
                {
                    Id = __entidad__.Id,
                    Nombre = __entidad__.Nombre,
                    Precio = __entidad__.Precio
                };
            }

            // Request -> Modelo nuevo
            public static __ENTIDAD__ A__ENTIDAD__(this __ENTIDAD__Request request)
            {
                return new __ENTIDAD__
                {
                    Nombre = request.Nombre,
                    Precio = request.Precio,
                    Activo = true
                };
            }
        }
        """;

    /// <summary>
    /// Archivo .http con las peticiones de ejemplo. Se puede ejecutar desde
    /// Visual Studio Code (extensión REST Client) o Visual Studio.
    /// </summary>
    public const string ArchivoHttp =
        """
        @host = http://localhost:5000

        ### Listar todos
        GET {{host}}/api/__entidad__

        ### Obtener por id
        GET {{host}}/api/__entidad__/1

        ### Crear
        POST {{host}}/api/__entidad__
        Content-Type: application/json

        {
          "nombre": "Ejemplo",
          "precio": 100
        }

        ### Actualizar
        PUT {{host}}/api/__entidad__/1
        Content-Type: application/json

        {
          "nombre": "Ejemplo modificado",
          "precio": 150
        }

        ### Eliminar
        DELETE {{host}}/api/__entidad__/1
        """;
}
