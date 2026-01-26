namespace Shared;
using System.ComponentModel.DataAnnotations;

public class Category
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required, MinLength(1)]
    public string Name { get; set; } = string.Empty;
}
