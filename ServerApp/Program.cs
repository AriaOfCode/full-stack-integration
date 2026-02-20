using System.ComponentModel.DataAnnotations;
using Shared;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(
        options => { 
            options.AddDefaultPolicy(policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
             }); });

// Abilita In-Memory Cache xche i dati non cambiano spesso inoltre migliora le performance e non ho più istanze del server

builder.Services.AddMemoryCache();

// In-memory "database" 
var productsDb = new List<Product> { new Product{ Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25, Category = new Category { Id = 1, Name = "Electronics" } }, new Product{ Id = 2, Name = "Headphones", Price = 50.00, Stock = 100, Category = new Category { Id = 1, Name = "Electronics" } }, new Product{ Id = 3, Name = "Smartphone", Price = 800.00, Stock = 50, Category = new Category { Id = 1, Name = "Electronics" } } };
var app = builder.Build();
app.UseCors();

const string cacheKey = "products_cache";
// ----------------------------------------
//                  GET PRODUCTS 
// ----------------------------------------

app.MapGet("/api/products", (IMemoryCache cache) =>
{   

if (cache.Get<List<Product>>(cacheKey) is { } cached)
 return Results.Ok(cached); 
 
 // Validazione dei prodotti 
    foreach (var p in productsDb) 
    {
         var validationResults = new List<ValidationResult>(); 
         var context = new ValidationContext(p); 
         if (!Validator.TryValidateObject(p, context, validationResults, true)) 
         {
             return Results.BadRequest(new { Message = "Validation failed for one or more products.", Errors = validationResults.Select(v => v.ErrorMessage) });
         }
     }
  // Cache per 5 minuti 
    cache.Set(cacheKey, productsDb, new MemoryCacheEntryOptions 
    {
         AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
         SlidingExpiration = TimeSpan.FromMinutes(2) 
   });
 return Results.Ok(productsDb);

});
 


// ----------------------------------------
//                  Post PRODUCTS 
// ----------------------------------------

app.MapPost("/api/products", (Product newProduct, IMemoryCache cache) =>
{
    // Validazione del prodotto in ingresso
    var validationResults = new List<ValidationResult>();
    var context = new ValidationContext(newProduct);

    bool isValid = Validator.TryValidateObject(
        newProduct,
        context,
        validationResults,
        validateAllProperties: true
    );

    if (!isValid)
    {
        // Restituisce un errore leggibile al client
        return Results.BadRequest(new
        {
            Message = "Validation failed for the product.",
            Errors = validationResults.Select(v => v.ErrorMessage)
        });
    }
// Aggiunta al "database" 
productsDb.Add(newProduct); 
// Invalida la cache cache.Remove(cacheKey);
return Results.Created($"/api/products/{newProduct.Id}", newProduct);


});

app.Run();
