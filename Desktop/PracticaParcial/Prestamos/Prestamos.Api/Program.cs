using Prestamos.LogicaServicio.Servicios;

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

// Inyección de dependencias (Unidad 6 - sección 13).
// AddSingleton: una sola instancia para toda la aplicación.
builder.Services.AddSingleton<ProductoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI en /openapi/v1.json
    app.MapOpenApi();
}

app.UseCors("PermitirTodo");
app.MapControllers();

app.Run();
