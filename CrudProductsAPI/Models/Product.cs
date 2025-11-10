using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudProductsAPI.Models;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name", TypeName = "varchar(255)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Column("description", TypeName = "text")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
}
