namespace UcseCLI.Templates;

/// <summary>
/// Plantillas de los archivos que acompañan a la solución: Program.cs de la
/// aplicación de consola, README y .gitignore.
/// </summary>
public static class PlantillasProyecto
{
    /// <summary>
    /// Program.cs del proyecto de consola: capa de presentación
    /// (Unidad 2 - sección 17). Solo muestra datos y toma entradas.
    /// </summary>
    public const string ProgramaDeConsola =
        """
        using System;
        using System.Collections.Generic;
        __USING_ENTIDADES____USING_SERVICIOS__
        // Capa de presentación (Unidad 2 - sección 17): muestra información y toma
        // entradas del usuario. Las reglas de negocio están en el Service, no acá.

        var servicio = new __ENTIDAD__Service();
        bool salir = false;

        while (!salir)
        {
            Console.WriteLine();
            Console.WriteLine("=== __PROYECTO__ ===");
            Console.WriteLine("1. Listar");
            Console.WriteLine("2. Agregar");
            Console.WriteLine("3. Eliminar");
            Console.WriteLine("0. Salir");
            Console.Write("Opción: ");

            string opcion = Console.ReadLine() ?? string.Empty;

            switch (opcion)
            {
                case "1":
                    Listar();
                    break;

                case "2":
                    Agregar();
                    break;

                case "3":
                    Eliminar();
                    break;

                case "0":
                    salir = true;
                    break;

                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }

        void Listar()
        {
            List<__ENTIDAD__> __entidades__ = servicio.ObtenerTodos();

            if (__entidades__.Count == 0)
            {
                Console.WriteLine("Todavía no hay datos cargados.");
                return;
            }

            foreach (__ENTIDAD__ __entidad__ in __entidades__)
            {
                Console.WriteLine($"  {__entidad__.Id} - {__entidad__.Nombre} (${__entidad__.Precio})");
            }
        }

        void Agregar()
        {
            Console.Write("Nombre: ");
            string nombre = Console.ReadLine() ?? string.Empty;

            Console.Write("Precio: ");
            decimal.TryParse(Console.ReadLine(), out decimal precio);

            try
            {
                __ENTIDAD__ creado = servicio.Agregar(
                    new __ENTIDAD__ { Nombre = nombre, Precio = precio });

                Console.WriteLine($"Agregado con Id {creado.Id}.");
            }
            catch (ArgumentException ex)
            {
                // El Service valida y lanza; la presentación solo informa.
                Console.WriteLine($"No se pudo agregar: {ex.Message}");
            }
        }

        void Eliminar()
        {
            Console.Write("Id a eliminar: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Id inválido.");
                return;
            }

            Console.WriteLine(servicio.Eliminar(id)
                ? "Eliminado."
                : "No existe un registro con ese Id.");
        }
        """;

    /// <summary>README de la solución generada.</summary>
    public const string Readme =
        """
        # __PROYECTO__

        Solución de Programación 1 (UCSE) generada con [UCSE CLI](https://github.com/BauDev0/UcseCLI).

        ## Estructura

        ```text
        __ESTRUCTURA__
        ```

        | Proyecto | Capa | Unidad |
        |---|---|---|
        __TABLA_PROYECTOS__

        ## Cómo ejecutar

        ```bash
        # restaurar dependencias y compilar
        dotnet build

        # correr los tests
        dotnet test
        __COMANDOS_EJECUCION__
        ```

        ## Dónde va cada cosa

        - **Entidades/**: clases del dominio (solo datos y comportamiento propio).
        - **Servicios/**: reglas de negocio. No escriben por consola ni leen input.
        - **Datos/**: lectura y escritura de los archivos JSON (Unidad 5).
        - **Tests**: un test por comportamiento, con nombres `Metodo_Escenario_ResultadoEsperado`.

        > El código de ejemplo (`__ENTIDAD__`) está para tomarlo como referencia:
        > reemplazalo por el modelo que pide el enunciado.
        """;

    /// <summary>.gitignore para proyectos .NET (Unidad 3 - Git).</summary>
    public const string GitIgnore =
        """
        ## .gitignore para proyectos .NET

        # Resultados de compilación
        [Bb]in/
        [Oo]bj/
        [Dd]ebug/
        [Rr]elease/
        x64/
        x86/
        [Ll]og/
        [Ll]ogs/
        artifacts/

        # Paquetes NuGet
        *.nupkg
        *.snupkg
        packages/

        # Archivos de usuario de Visual Studio / Rider / VS Code
        *.user
        *.suo
        *.userosscache
        *.sln.docstates
        .vs/
        .vscode/
        .idea/
        *.DotSettings.user

        # Resultados de tests y cobertura
        [Tt]est[Rr]esult*/
        coverage*.json
        coverage*.xml
        *.coverage
        *.trx

        # Variables de entorno y secretos
        .env
        appsettings.*.local.json

        # Sistema operativo
        .DS_Store
        Thumbs.db
        """;
}
