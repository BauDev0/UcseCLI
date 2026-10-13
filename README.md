# UCSE CLI

Herramienta de línea de comandos que **automatiza el armado de proyectos** de la materia
**Programación 1 (UCSE)**. Crea la estructura de carpetas y proyectos, configura las
referencias entre ellos y deja **código de ejemplo para reutilizar y adaptar**, siguiendo
las convenciones del [repositorio oficial de la materia](https://github.com/UCSE-PROG1/programacion-1).

> La herramienta automatiza lo repetitivo: soluciones, proyectos, referencias, paquetes y
> archivos base. **No resuelve el enunciado**: el análisis del dominio, las reglas de
> negocio y los tests de esas reglas siguen siendo tu trabajo.

---

## Requisitos

* .NET SDK 8 o superior (la materia usa **.NET 8**).
* Git (opcional, para que la herramienta inicialice el repositorio).

```bash
dotnet --version
```

---

## Instalación

```bash
dotnet tool install --global UcseCLI
```

Actualizar:

```bash
dotnet tool update --global UcseCLI
```

Verificar:

```bash
ucse info
```

`ucse info` muestra el SDK detectado y la lista completa de comandos.

---

## Qué genera

La estructura sigue la arquitectura por capas de los apuntes
(Unidad 2 - sección 17, Unidad 5 - sección 10 y Unidad 6 - sección 18):

| Capa | Dónde vive | Responsabilidad |
|---|---|---|
| Presentación | `*.Consola` / `*.Api` | Muestra datos y recibe pedidos. No tiene reglas de negocio. |
| Lógica | `*.LogicaServicio/Servicios` | Reglas del negocio. No escribe por consola ni lee input. |
| Datos | `*.LogicaServicio/Datos` | Lectura y escritura de los archivos JSON. |
| Entidades | `*.LogicaServicio/Entidades` | Clases del dominio (POCO). |
| Tests | `*.LogicaTest` | Tests NUnit con patrón Arrange-Act-Assert. |

---

## Crear una solución por capas

Para las Unidades 1 a 5 (consola + lógica + tests):

```bash
ucse new SistemaVentas
```

```text
SistemaVentas
├── SistemaVentas.sln
├── .gitignore
├── README.md
│
├── SistemaVentas.Consola            → presentación (dotnet new console)
│   └── Program.cs                   menú de ejemplo
│
├── SistemaVentas.LogicaServicio     → lógica (dotnet new classlib)
│   ├── Entidades/Producto.cs
│   ├── Servicios/ProductoService.cs
│   └── Datos/ProductoRepositorio.cs
│
└── SistemaVentas.LogicaTest         → tests (dotnet new nunit)
    └── ProductoServiceTest.cs
```

Además de crear los proyectos, la herramienta:

* crea la solución y agrega los tres proyectos;
* referencia `LogicaServicio` desde `Consola` y desde `LogicaTest`;
* agrega el paquete `Newtonsoft.Json` a la lógica (Unidad 5);
* escribe un `.gitignore` de .NET y un `README.md` con la estructura;
* inicializa el repositorio Git.

### Opciones

| Opción | Qué hace |
|---|---|
| `-e`, `--entidad <Nombre>` | Entidad del código de ejemplo (por defecto `Producto`) |
| `-f`, `--framework <tfm>` | Target framework, por ejemplo `net8.0` (por defecto, el del SDK) |
| `--con-api` | Agrega además un proyecto Web API |
| `--sin-consola` | Solo lógica y tests |
| `--sin-ejemplos` | Genera la estructura sin código de ejemplo |
| `--sin-git` | No inicializa el repositorio |

```bash
ucse new HelpDesk --entidad Ticket --framework net8.0
```

---

## Crear una solución con Web API

Para la Unidad 6 (API + lógica + tests):

```bash
ucse new-webapi SistemaVentas
```

```text
SistemaVentas
├── SistemaVentas.sln
│
├── SistemaVentas.Api                → Web API con controllers
│   ├── Controllers/ProductoController.cs
│   ├── DTOs/ProductoRequest.cs
│   ├── DTOs/ProductoResponse.cs
│   ├── DTOs/ProductoMapper.cs
│   ├── SistemaVentas.Api.http       peticiones de ejemplo
│   └── Program.cs                   controllers + CORS + inyección de dependencias
│
├── SistemaVentas.LogicaServicio
│   ├── Entidades/Producto.cs
│   ├── Servicios/ProductoService.cs
│   └── Datos/ProductoRepositorio.cs
│
└── SistemaVentas.LogicaTest
    └── ProductoServiceTest.cs
```

El proyecto se crea con **controllers** (`dotnet new webapi --use-controllers`), porque la
plantilla de .NET genera Minimal APIs por defecto y la materia trabaja con controllers.
`Program.cs` queda con `AddControllers()`, CORS habilitado para el frontend y el servicio
registrado en el contenedor de dependencias.

Los DTOs viven en el proyecto de la API y el mapeo se hace en el controller: el servicio
solo conoce la entidad, nunca los DTOs.

Agregar `--con-consola` si además se quiere la aplicación de consola.

---

## Sumar una Web API a una solución que ya existe

Es el paso de la práctica de la Unidad 6 sobre una solución armada en las Unidades 4 y 5.
Se ejecuta en la raíz de la solución:

```bash
cd HelpDesk
ucse add-api
```

Crea `HelpDesk.Api`, lo agrega a la solución, lo referencia contra el proyecto de lógica y
genera el controller con sus DTOs. La entidad se detecta a partir de los servicios
existentes; si hay varias, se indica con `--entidad Ticket`.

---

## Crear el frontend

Para la Unidad 7 (HTML, CSS y JavaScript sin frameworks):

```bash
ucse new-frontend Frontend --entidad Producto
```

```text
Frontend
├── index.html     formulario y tabla
├── styles.css
├── app.js         GET, POST y DELETE con fetch
└── README.md
```

Con `--api http://localhost:5050/api/producto` se fija la URL del endpoint.

---

## Agregar archivos a un proyecto

Todos estos comandos **deducen el namespace** a partir del `.csproj` y de la carpeta
destino (se puede forzar con `-n`, `--namespace`).

| Comando | Genera |
|---|---|
| `ucse add-clase <ruta> <Nombres...>` | Clases vacías |
| `ucse add-entidad <ruta> <Nombres...>` | Entidades del dominio |
| `ucse add-interfaz <ruta> <Nombres...>` | Interfaces (agrega el prefijo `I`) |
| `ucse add-abstracta <ruta> <Nombres...>` | Clases abstractas |
| `ucse add-enum <ruta> <Nombres...>` | Enumeradores |
| `ucse add-servicio <ruta> <Entidad>` | Servicio con las operaciones básicas |
| `ucse add-repositorio <ruta> <Entidad>` | Repositorio con persistencia JSON |
| `ucse add-test <ruta> <Clase>` | `TestFixture` de NUnit con patrón AAA |
| `ucse add-controller <proyecto-api> <Entidad>` | Controller REST + Request + Response + Mapper |

```bash
ucse add-entidad SistemaVentas.LogicaServicio/Entidades Cliente Factura
```

```text
SistemaVentas.LogicaServicio/Entidades
├── Cliente.cs        namespace SistemaVentas.LogicaServicio.Entidades
└── Factura.cs
```

Los archivos existentes **no se sobreescriben**: se avisa y se dejan como están.

`add-test` reconoce el sufijo `Service` y, en ese caso, genera la batería de tests
completa (alta, validación, búsqueda inexistente, baja y persistencia) sobre un archivo
JSON temporal, para que los tests queden aislados entre sí.

`add-clase` también responde al nombre `add-files`, como en las versiones anteriores.

---

## Ejemplos de código para reutilizar

```bash
ucse ejemplos
```

| Tema | Contenido | Unidad |
|---|---|---|
| `herencia` | Clase abstracta, `override` y polimorfismo | 2 |
| `interfaz` | Interfaz, implementaciones e inyección por constructor | 2 |
| `excepciones` | `ArgumentException` vs `InvalidOperationException`, `try/catch` | 2 |
| `linq` | `Where`, `Select`, `OrderBy`, agregados y `GroupBy` | 2 |
| `json` | Repositorio con `JsonConvert` y `File` | 5 |
| `nunit` | AAA, `Assert.Throws`, `TestCase` y `Within` | 4 |
| `controller` | Controller REST con DTOs y códigos de estado | 6 |
| `fetch` | `fetch` con GET, POST y DELETE, y armado del DOM | 7 |
| `git` | Ramas, commits y pull requests | 3 |

```bash
ucse ejemplos linq                        # lo muestra en la terminal
ucse ejemplos json --guardar Notas/json.cs # lo guarda en un archivo
```

---

## Otros comandos

| Comando | Qué hace |
|---|---|
| `ucse info` | SDK detectado, estado de Git y guía de comandos |
| `ucse gitignore` | Crea un `.gitignore` de .NET en la carpeta actual |
| `ucse <comando> --help` | Opciones del comando |
| `ucse --version` | Versión de la herramienta |

---

## Flujo de trabajo recomendado

```bash
# 1. Armar la solución con la lógica, los tests y la consola
ucse new HelpDesk --entidad Ticket
cd HelpDesk

# 2. Modelar el dominio del enunciado
ucse add-enum HelpDesk.LogicaServicio/Entidades Prioridad Estado
ucse add-entidad HelpDesk.LogicaServicio/Entidades Usuario

# 3. Escribir las reglas de negocio y sus tests
ucse add-servicio HelpDesk.LogicaServicio/Servicios Usuario
ucse add-test HelpDesk.LogicaTest UsuarioService
dotnet test

# 4. Exponer la lógica por HTTP (Unidad 6)
ucse add-api
dotnet run --project HelpDesk.Api

# 5. Sumar el frontend (Unidad 7)
ucse new-frontend Frontend --entidad Ticket
```

---

## Tecnologías utilizadas

* C# / .NET 10 (la herramienta) y .NET 8 o superior (los proyectos generados)
* System.CommandLine
* Spectre.Console
* NUnit y Newtonsoft.Json en los proyectos generados
* Distribución como .NET Tool por NuGet

---

## Estructura del repositorio

```text
UcseCLI/UcseCLI
├── Program.cs          punto de entrada
├── Cli/                definición de los comandos y los comandos informativos
├── Core/               validaciones, rutas seguras, ejecución de dotnet y git, salida
├── Generators/         armado de soluciones, API, frontend y archivos sueltos
└── Templates/          plantillas de código y catálogo de ejemplos
```

Cada plantilla es texto literal con marcadores (`__ENTIDAD__`, `__NS__`, ...), así el
código de ejemplo se lee igual que el archivo que se va a generar.

---

## Roadmap

- [x] Integración con Web-API
- [x] Generación de interfaces
- [x] Generación de clases abstractas y enumeradores
- [x] Arquitecturas multicapa (presentación / lógica / datos)
- [x] Persistencia en JSON y tests de ejemplo
- [x] Frontend HTML + CSS + JS (Unidad 7)
- [x] Integración con Git (`git init` y `.gitignore`)
- [ ] Plantillas personalizadas definidas por el usuario
- [ ] Generación de clientes HTTP (`HttpClient`) para consumir APIs

---

## Autor

**Bautista**

Proyecto desarrollado para automatizar tareas repetitivas durante la cursada y acelerar la
creación de proyectos académicos.

---

## Licencia

Este proyecto se distribuye bajo licencia MIT.
