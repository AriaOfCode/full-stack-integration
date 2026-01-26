using System.ComponentModel.DataAnnotations;
using Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(
        options => { 
            options.AddDefaultPolicy(policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
             }); });


var app = builder.Build();
app.UseCors();
app.MapGet("/api/products", () =>

{
    
        var products = GetProducts();

    foreach (var p in products)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(p);

        bool isValid = Validator.TryValidateObject(
            p,
            context,
            validationResults,
            validateAllProperties: true
        );

        if (!isValid)
        {
            // Restituisce un errore leggibile al client
            return Results.BadRequest(new
            {
                Message = "Validation failed for one or more products.",
                Errors = validationResults.Select(v => v.ErrorMessage)
            });
        }
    }

    return Results.Ok(products);

    

});

static List<Product> GetProducts()
{
    return new List<Product>
    {

        new Product{ Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25, Category = new Category { Id = 1, Name = "Electronics" } },

        new Product { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100, Category = new Category { Id = 1, Name = "Electronics" } },
        new Product { Id = 3, Name = "Smartphone", Price = 800.00, Stock = 50, Category = new Category { Id = 1, Name = "Electronics" } }

    };
}

app.Run();
