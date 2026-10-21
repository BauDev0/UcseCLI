# Frontend - Frontend

Frontend de ejemplo en HTML, CSS y JavaScript sin frameworks (Unidad 7).

## Cómo abrirlo

1. Levantar la Web API (desde la carpeta de la solución):

   ```bash
   dotnet run --project MiSolucion.Api
   ```

2. Confirmar en la consola en qué puerto quedó escuchando y, si no coincide,
   ajustar `API_URL` en `app.js`:

   ```js
   const API_URL = "http://localhost:5000/api/producto";
   ```

3. Abrir `index.html` en el navegador. Con la extensión **Live Server** de
   VS Code se recarga solo al guardar.

> Si el navegador bloquea los pedidos por CORS, revisar que la API tenga
> `app.UseCors("PermitirTodo")` antes de `app.MapControllers()`.
