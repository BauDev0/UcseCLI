# Prestamos

Solución de Programación 1 (UCSE) generada con [UCSE CLI](https://github.com/BauDev0/UcseCLI).

## Estructura

```text
Prestamos.Api/
Prestamos.LogicaServicio/
Prestamos.LogicaTest/
```

| Proyecto | Capa | Unidad |
|---|---|---|
| `Prestamos.Api` | Web API (controllers y DTOs) | 6 |
| `Prestamos.LogicaServicio` | Lógica de negocio y datos | 2, 5 |
| `Prestamos.LogicaTest` | Tests unitarios (NUnit) | 4 |

## Cómo ejecutar

```bash
# restaurar dependencias y compilar
dotnet build

# correr los tests
dotnet test

# levantar la Web API
dotnet run --project Prestamos.Api
```

## Dónde va cada cosa

- **Entidades/**: clases del dominio (solo datos y comportamiento propio).
- **Servicios/**: reglas de negocio. No escriben por consola ni leen input.
- **Datos/**: lectura y escritura de los archivos JSON (Unidad 5).
- **Tests**: un test por comportamiento, con nombres `Metodo_Escenario_ResultadoEsperado`.

> El código de ejemplo (`Producto`) está para tomarlo como referencia:
> reemplazalo por el modelo que pide el enunciado.
