using System.ComponentModel.DataAnnotations;

namespace CrudProductsAPI.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "El campo 'name' es requerido")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 255 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo 'description' es requerido")]
    [MinLength(1, ErrorMessage = "La descripción no puede estar vacía")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo 'price' es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser un número positivo mayor a 0")]
    public decimal Price { get; set; }
}
