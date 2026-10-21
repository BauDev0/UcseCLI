using System.ComponentModel.DataAnnotations;

namespace Prestamos.Api.DTOs;

/// <summary>
/// Request: lo que manda el cliente en el body de un POST o PUT.
/// No incluye el Id (en POST lo genera el servidor, en PUT viene en la URL).
/// Las validaciones van acá, no en la entidad (Unidad 6 - sección 10).
/// </summary>
public class ProductoRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }
}
