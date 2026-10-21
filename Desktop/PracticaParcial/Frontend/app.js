// Frontend de ejemplo (Unidad 7): HTML + CSS + JavaScript sin frameworks.
// Consume la Web API con fetch. La API tiene que estar corriendo y con CORS
// habilitado (ver Program.cs del proyecto Api).

const API_URL = "http://localhost:5000/api/producto";

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
