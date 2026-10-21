using UcseCLI.Core;
using UcseCLI.Templates;

namespace UcseCLI.Generators;

/// <summary>
/// Escribe los archivos del proyecto Web API: Program.cs con controllers,
/// controller de ejemplo, DTOs (Request/Response), mapper y archivo .http.
/// </summary>
public static class GeneradorDeWebApi
{
    public static void Escribir(
        EscritorDeArchivos escritor,
        string raiz,
        string logica,
        string api,
        string entidad,
        bool conEjemplos,
        bool ejemplosActivos)
    {
        var carpetaApi = Path.Combine(raiz, api);
        var nsEntidades = $"{logica}.Entidades";
        var nsServicios = $"{logica}.Servicios";
        var nsControllers = $"{api}.Controllers";
        var nsDtos = $"{api}.DTOs";

        Ui.Paso("Configurando Program.cs (controllers, CORS e inyección de dependencias)");

        escritor.Escribir(
            Path.Combine(carpetaApi, "Program.cs"),
            GeneradorDeSolucion.Render(ElegirPlantillaDeProgram(carpetaApi, api), entidad,
                ("__USING_SERVICIOS__", conEjemplos && ejemplosActivos
                    ? Plantilla.Usings(nsServicios)
                    : string.Empty),
                ("__REGISTRO_SERVICIOS__",
                    RegistroDeServicios(entidad, nsServicios, conEjemplos, ejemplosActivos))));

        if (!conEjemplos)
        {
            return;
        }

        Ui.Paso(ejemplosActivos
            ? $"Generando controller y DTOs de {entidad}"
            : $"Generando controller y DTOs de {entidad} (comentados)");

        escritor.Escribir(
            Path.Combine(carpetaApi, "Controllers", $"{entidad}Controller.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.Controlador, entidad,
                    ("__NS__", Plantilla.Namespace(nsControllers)),
                    ("__USING_DTOS__", Plantilla.Usings(nsDtos)),
                    ("__USING_ENTIDADES__", Plantilla.Usings(nsEntidades)),
                    ("__USING_SERVICIOS__", Plantilla.Usings(nsServicios))),
                $"Controller REST de {entidad} (Unidad 6)",
                ejemplosActivos));

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Request.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.DtoRequest, entidad,
                    ("__NS__", Plantilla.Namespace(nsDtos))),
                $"DTO de entrada de {entidad} (Unidad 6)",
                ejemplosActivos));

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Response.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.DtoResponse, entidad,
                    ("__NS__", Plantilla.Namespace(nsDtos))),
                $"DTO de salida de {entidad} (Unidad 6)",
                ejemplosActivos));

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Mapper.cs"),
            Plantilla.SegunModo(
                GeneradorDeSolucion.Render(PlantillasWebApi.Mapper, entidad,
                    ("__NS__", Plantilla.Namespace(nsDtos)),
                    ("__USING_ENTIDADES__", Plantilla.Usings(nsEntidades))),
                $"Mapeo entre {entidad} y sus DTOs (Unidad 6)",
                ejemplosActivos));

        // La plantilla de dotnet deja un .http con el endpoint de ejemplo que se borró.
        escritor.Escribir(
            Path.Combine(carpetaApi, $"{api}.http"),
            GeneradorDeSolucion.Render(PlantillasWebApi.ArchivoHttp, entidad));
    }

    /// <summary>
    /// Elige la variante de Program.cs según lo que referencie el .csproj:
    /// Swashbuckle (Swagger UI en /swagger) o Microsoft.AspNetCore.OpenApi
    /// (documento OpenAPI en /openapi/v1.json).
    /// </summary>
    private static string ElegirPlantillaDeProgram(string carpetaApi, string api)
    {
        var csproj = Path.Combine(carpetaApi, $"{api}.csproj");

        if (ProyectoCs.TienePaquete(csproj, "Swashbuckle.AspNetCore"))
        {
            return PlantillasWebApi.ProgramaConSwagger;
        }

        if (ProyectoCs.TienePaquete(csproj, "Microsoft.AspNetCore.OpenApi"))
        {
            Ui.Nota("La plantilla del SDK usa Microsoft.AspNetCore.OpenApi: el documento queda en /openapi/v1.json.");
            Ui.Nota("Para la interfaz de Swagger: dotnet add " + Path.Combine(api, $"{api}.csproj") + " package Swashbuckle.AspNetCore");

            return PlantillasWebApi.ProgramaConOpenApi;
        }

        // Sin paquete de documentación: queda la API con controllers, sin Swagger.
        Ui.Nota("El proyecto no incluye paquete de OpenAPI: la API queda con controllers, sin Swagger.");

        return PlantillasWebApi.ProgramaSinOpenApi;
    }

    private static string RegistroDeServicios(
        string entidad,
        string nsServicios,
        bool conEjemplos,
        bool ejemplosActivos)
    {
        if (!conEjemplos)
        {
            return "// TODO: registrar acá los servicios de tu lógica." + Environment.NewLine +
                   "// builder.Services.AddSingleton<MiService>();";
        }

        var comentario =
            "// Inyección de dependencias (Unidad 6 - sección 13)." + Environment.NewLine +
            "// AddSingleton: una sola instancia para toda la aplicación." + Environment.NewLine;

        if (ejemplosActivos)
        {
            return comentario + $"builder.Services.AddSingleton<{entidad}Service>();";
        }

        // Con el servicio de ejemplo comentado, su registro también va comentado:
        // si no, el proyecto no compilaría.
        return comentario +
               $"// using {nsServicios};  (va arriba de todo, junto a los demás using)" + Environment.NewLine +
               $"// builder.Services.AddSingleton<{entidad}Service>();";
    }
}
