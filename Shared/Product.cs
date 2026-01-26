namespace Shared;
using System.ComponentModel.DataAnnotations;

public class Product
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required, MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public double Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    public Category? Category { get; set; }  
}
