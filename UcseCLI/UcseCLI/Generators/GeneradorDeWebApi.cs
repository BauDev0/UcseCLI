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
        bool conEjemplos)
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
                ("__USING_SERVICIOS__", conEjemplos ? Plantilla.Usings(nsServicios) : string.Empty),
                ("__REGISTRO_SERVICIOS__", RegistroDeServicios(entidad, conEjemplos))));

        if (!conEjemplos)
        {
            return;
        }

        Ui.Paso($"Generando controller y DTOs de {entidad}");

        escritor.Escribir(
            Path.Combine(carpetaApi, "Controllers", $"{entidad}Controller.cs"),
            GeneradorDeSolucion.Render(PlantillasWebApi.Controlador, entidad,
                ("__NS__", Plantilla.Namespace(nsControllers)),
                ("__USING_DTOS__", Plantilla.Usings(nsDtos)),
                ("__USING_ENTIDADES__", Plantilla.Usings(nsEntidades)),
                ("__USING_SERVICIOS__", Plantilla.Usings(nsServicios))));

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Request.cs"),
            GeneradorDeSolucion.Render(PlantillasWebApi.DtoRequest, entidad,
                ("__NS__", Plantilla.Namespace(nsDtos))));

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Response.cs"),
            GeneradorDeSolucion.Render(PlantillasWebApi.DtoResponse, entidad,
                ("__NS__", Plantilla.Namespace(nsDtos))));

        escritor.Escribir(
            Path.Combine(carpetaApi, "DTOs", $"{entidad}Mapper.cs"),
            GeneradorDeSolucion.Render(PlantillasWebApi.Mapper, entidad,
                ("__NS__", Plantilla.Namespace(nsDtos)),
                ("__USING_ENTIDADES__", Plantilla.Usings(nsEntidades))));

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

    private static string RegistroDeServicios(string entidad, bool conEjemplos) =>
        conEjemplos
            ? "// Inyección de dependencias (Unidad 6 - sección 13)." + Environment.NewLine +
              "// AddSingleton: una sola instancia para toda la aplicación." + Environment.NewLine +
              $"builder.Services.AddSingleton<{entidad}Service>();"
            : "// TODO: registrar acá los servicios de tu lógica." + Environment.NewLine +
              "// builder.Services.AddSingleton<MiService>();";
}
