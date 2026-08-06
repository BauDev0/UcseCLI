# UCSE CLI

UCSE CLI es una herramienta de línea de comandos desarrollada en C# y .NET 10 que automatiza la creación de soluciones y clases utilizadas durante la cursada de programación.

Su objetivo es reducir el tiempo invertido en tareas repetitivas para que los estudiantes puedan enfocarse en desarrollar la lógica de negocio y resolver problemas.

---

## Características

* Creación automática de soluciones .NET.
* Creación de proyectos de lógica (`Class Library`).
* Creación de proyectos de pruebas con NUnit.
* Generación automática de archivos `.cs`.
* Creación de soluciones con proyectos Web API de ejemplo.
* Configuración automática de referencias entre proyectos.
* Distribución mediante NuGet Tool.

---

## Requisitos

* .NET SDK 10 o superior.

Verificar instalación:

```bash
dotnet --version
```

---

## Instalación

Instalar la herramienta globalmente:

```bash
dotnet tool install --global UcseCLI
```

Actualizar a la última versión:

```bash
dotnet tool update --global UcseCLI
```

Verificar instalación:

```bash
ucse --help
```

---

## Crear una Solución

### Comando

```bash
ucse new SistemaVentas
```

### Resultado

```text
SistemaVentas
│
├── SistemaVentas.LogicaServicio
│
├── SistemaVentas.LogicaTest
│
└── SolucionGeneral.slnx
```

### Configuración Automática

La herramienta realiza automáticamente:

* Creación del proyecto de lógica.
* Creación del proyecto de pruebas NUnit.
* Creación de la solución `.slnx`.
* Inclusión de los proyectos dentro de la solución.
* Referencia del proyecto de pruebas hacia el proyecto de lógica.

---

## Crear una Solución Web API

### Comando

```bash
ucse new-webapi SistemaVentas
```

### Resultado

```text
SistemaVentas
│
├── SistemaVentas.Api
│   ├── Controllers
│   │   └── ProductoController.cs
│   ├── DTOs
│   │   ├── ProductoRequest.cs
│   │   ├── ProductoResponse.cs
│   │   └── ProductoMapper.cs
│   ├── appsettings.json
│   └── Program.cs
│
├── SistemaVentas.LogicaServicio
│   ├── Entidades
│   │   └── Producto.cs
│   └── Servicios
│       └── ProductoService.cs
│
├── SistemaVentas.LogicaTest
│
└── SolucionGeneral.slnx
```

### Configuración Automática

La herramienta realiza automáticamente:

* Creación del proyecto Web API.
* Creación del proyecto de lógica.
* Creación del proyecto de pruebas NUnit.
* Creación de la solución `.slnx`.
* Inclusión de los proyectos dentro de la solución.
* Referencia del proyecto de API hacia el proyecto de lógica.
* Referencia del proyecto de pruebas hacia el proyecto de lógica.
* Generación de DTOs, mapper y controller de ejemplo.
* Registro del servicio en `Program.cs`.

---

## Crear Múltiples Clases

### Comando

```bash
ucse add-files Models Cliente Producto Factura
```

### Resultado

```text
Models
├── Cliente.cs
├── Producto.cs
└── Factura.cs
```

### Clase Generada

```csharp
public class Cliente
{
}
```

---

## Flujo de Trabajo Recomendado

Crear una nueva solución:

```bash
ucse new SistemaBiblioteca
```

Ingresar al proyecto:

```bash
cd SistemaBiblioteca
```

Crear entidades:

```bash
ucse add-files Models Libro Autor Prestamo Usuario
```

Resultado:

```text
Models
├── Libro.cs
├── Autor.cs
├── Prestamo.cs
└── Usuario.cs
```

---

## Tecnologías Utilizadas

* C#
* .NET 10
* System.CommandLine
* Spectre.Console
* NUnit
* NuGet

---

## Ejemplo de Estructura Generada

```text
SistemaBiblioteca
│
├── SistemaBiblioteca.LogicaServicio
│
├── SistemaBiblioteca.LogicaTest
│
├── Models
│   ├── Libro.cs
│   ├── Autor.cs
│   ├── Prestamo.cs
│   └── Usuario.cs
│
└── SolucionGeneral.slnx
```

---

## Roadmap

Próximas funcionalidades:

- [x] Integración con Web-API
- [ ] Generación de interfaces.
- [ ] Generación de clases abstractas.
- [ ] Arquitecturas multicapa.
- [ ] Plantillas personalizadas.
- [ ] Integración con Git.

---

## Autor

**Bautista**

Proyecto desarrollado para automatizar tareas repetitivas durante la cursada y acelerar la creación de proyectos académicos.

---

## Licencia

Este proyecto se distribuye bajo licencia MIT.
