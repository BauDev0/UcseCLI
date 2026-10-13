namespace UcseCLI.Templates;

/// <summary>
/// Plantillas del frontend (Unidad 7): HTML, CSS y JavaScript sin frameworks,
/// consumiendo la Web API con fetch.
/// </summary>
public static class PlantillasFrontend
{
    public const string IndexHtml =
        """
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>__PROYECTO__</title>
            <link rel="stylesheet" href="styles.css">
        </head>
        <body>
            <header>
                <h1>__PROYECTO__</h1>
                <p id="estado" class="estado"></p>
            </header>

            <main>
                <section>
                    <h2>Nuevo registro</h2>

                    <!-- El form no recarga la página: el submit se maneja en app.js -->
                    <form id="formulario" class="tarjeta">
                        <label for="nombre">Nombre</label>
                        <input id="nombre" name="nombre" type="text" required maxlength="100">

                        <label for="precio">Precio</label>
                        <input id="precio" name="precio" type="number" min="0.01" step="0.01" required>

                        <button type="submit">Guardar</button>
                    </form>
                </section>

                <section>
                    <h2>Listado</h2>
                    <button id="recargar" type="button">Recargar</button>

                    <table class="tabla">
                        <thead>
                            <tr>
                                <th>Id</th>
                                <th>Nombre</th>
                                <th>Precio</th>
                                <th></th>
                            </tr>
                        </thead>
                        <!-- Las filas se generan desde JavaScript -->
                        <tbody id="cuerpo-tabla"></tbody>
                    </table>
                </section>
            </main>

            <script src="app.js"></script>
        </body>
        </html>
        """;

    public const string StylesCss =
        """
        :root {
            --color-texto: #1f2933;
            --color-borde: #d9e2ec;
            --color-acento: #2563eb;
            --color-error: #b91c1c;
            --color-ok: #15803d;
        }

        * {
            box-sizing: border-box;
        }

        body {
            margin: 0;
            padding: 24px;
            font-family: system-ui, "Segoe UI", Roboto, sans-serif;
            color: var(--color-texto);
            background-color: #f8fafc;
            line-height: 1.5;
        }

        header {
            margin-bottom: 24px;
        }

        h1 {
            margin: 0 0 4px;
            font-size: 1.6rem;
        }

        h2 {
            font-size: 1.1rem;
            margin-bottom: 8px;
        }

        main {
            display: grid;
            gap: 24px;
            max-width: 900px;
        }

        .tarjeta {
            display: grid;
            gap: 8px;
            max-width: 320px;
            padding: 16px;
            background-color: #ffffff;
            border: 1px solid var(--color-borde);
            border-radius: 8px;
        }

        label {
            font-size: 0.85rem;
            font-weight: 600;
        }

        input {
            padding: 8px;
            border: 1px solid var(--color-borde);
            border-radius: 4px;
            font-size: 0.95rem;
        }

        button {
            padding: 8px 14px;
            border: none;
            border-radius: 4px;
            background-color: var(--color-acento);
            color: #ffffff;
            font-size: 0.9rem;
            cursor: pointer;
        }

        button:hover {
            opacity: 0.9;
        }

        button.secundario {
            background-color: #64748b;
        }

        .tabla {
            width: 100%;
            margin-top: 12px;
            border-collapse: collapse;
            background-color: #ffffff;
        }

        .tabla th,
        .tabla td {
            padding: 8px 12px;
            border-bottom: 1px solid var(--color-borde);
            text-align: left;
        }

        .estado {
            min-height: 1.2rem;
            margin: 0;
            font-size: 0.9rem;
        }

        .estado.error {
            color: var(--color-error);
        }

        .estado.ok {
            color: var(--color-ok);
        }
        """;

    public const string AppJs =
        """
        // Frontend de ejemplo (Unidad 7): HTML + CSS + JavaScript sin frameworks.
        // Consume la Web API con fetch. La API tiene que estar corriendo y con CORS
        // habilitado (ver Program.cs del proyecto Api).

        const API_URL = "__API_URL__";

        const formulario = document.getElementById("formulario");
        const cuerpoTabla = document.getElementById("cuerpo-tabla");
        const estado = document.getElementById("estado");
        const botonRecargar = document.getElementById("recargar");

        // GET: trae el listado y lo dibuja en la tabla.
        async function cargar() {
            mostrarEstado("Cargando...");

            try {
                const respuesta = await fetch(API_URL);

                if (!respuesta.ok) {
                    throw new Error(`La API respondió ${respuesta.status}`);
                }

                const datos = await respuesta.json();
                dibujar(datos);
                mostrarEstado(`${datos.length} registro(s)`, "ok");
            } catch (error) {
                mostrarEstado(`No se pudo conectar con la API: ${error.message}`, "error");
            }
        }

        function dibujar(datos) {
            cuerpoTabla.innerHTML = "";

            for (const item of datos) {
                const fila = document.createElement("tr");

                // textContent (en lugar de innerHTML) evita inyectar HTML desde los datos.
                const celdaId = document.createElement("td");
                celdaId.textContent = item.id;

                const celdaNombre = document.createElement("td");
                celdaNombre.textContent = item.nombre;

                const celdaPrecio = document.createElement("td");
                celdaPrecio.textContent = item.precio;

                const celdaAcciones = document.createElement("td");
                const boton = document.createElement("button");
                boton.textContent = "Eliminar";
                boton.className = "secundario";
                boton.addEventListener("click", () => eliminar(item.id));
                celdaAcciones.appendChild(boton);

                fila.append(celdaId, celdaNombre, celdaPrecio, celdaAcciones);
                cuerpoTabla.appendChild(fila);
            }
        }

        // POST: crea un registro nuevo con los datos del formulario.
        formulario.addEventListener("submit", async (evento) => {
            evento.preventDefault();

            const cuerpo = {
                nombre: document.getElementById("nombre").value,
                precio: Number(document.getElementById("precio").value)
            };

            try {
                const respuesta = await fetch(API_URL, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(cuerpo)
                });

                if (!respuesta.ok) {
                    throw new Error(`La API respondió ${respuesta.status}`);
                }

                formulario.reset();
                await cargar();
            } catch (error) {
                mostrarEstado(`No se pudo guardar: ${error.message}`, "error");
            }
        });

        // DELETE: borra el registro elegido.
        async function eliminar(id) {
            try {
                const respuesta = await fetch(`${API_URL}/${id}`, { method: "DELETE" });

                if (!respuesta.ok) {
                    throw new Error(`La API respondió ${respuesta.status}`);
                }

                await cargar();
            } catch (error) {
                mostrarEstado(`No se pudo eliminar: ${error.message}`, "error");
            }
        }

        function mostrarEstado(mensaje, clase = "") {
            estado.textContent = mensaje;
            estado.className = `estado ${clase}`;
        }

        botonRecargar.addEventListener("click", cargar);

        // Primera carga al abrir la página.
        cargar();
        """;

    /// <summary>Instrucciones para abrir el frontend sin servidor web.</summary>
    public const string ReadmeFrontend =
        """
        # Frontend - __PROYECTO__

        Frontend de ejemplo en HTML, CSS y JavaScript sin frameworks (Unidad 7).

        ## Cómo abrirlo

        1. Levantar la Web API (desde la carpeta de la solución):

           ```bash
           dotnet run --project __PROYECTO_API__
           ```

        2. Confirmar en la consola en qué puerto quedó escuchando y, si no coincide,
           ajustar `API_URL` en `app.js`:

           ```js
           const API_URL = "__API_URL__";
           ```

        3. Abrir `index.html` en el navegador. Con la extensión **Live Server** de
           VS Code se recarga solo al guardar.

        > Si el navegador bloquea los pedidos por CORS, revisar que la API tenga
        > `app.UseCors("PermitirTodo")` antes de `app.MapControllers()`.
        """;
}
