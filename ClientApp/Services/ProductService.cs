using Shared;
using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;

public class ProductService
{
    private readonly HttpClient _http;

    public ProductService(HttpClient http)
    {
        _http = http;
    }

     public async Task<(List<Product>? Data, string? Error)> GetProductsAsync()
{
    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var response = await _http.GetAsync("http://localhost:5070/api/products", cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            return (null, $"API error: {response.StatusCode}");
        }

        var products = await response.Content.ReadFromJsonAsync<List<Product>>(cancellationToken: cts.Token);

        if (products == null)
        {
            return (null, "Invalid JSON: cannot deserialize product list.");
        }

        // 🔍 VALIDAZIONE LATO CLIENT
        var validationErrors = new List<string>();

        foreach (var p in products)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(p);

            bool isValid = Validator.TryValidateObject(
                p,
                context,
                results,
                validateAllProperties: true
            );

            if (!isValid)
            {
                validationErrors.AddRange(results.Select(r => r.ErrorMessage ?? "Validation error"));
            }
        }

        if (validationErrors.Any())
        {
            // Restituisce tutti gli errori concatenati
            return (null, string.Join(" | ", validationErrors));
        }

        return (products, null);
    }
    catch (TaskCanceledException)
    {
        return (null, "Request timed out");
    }
    catch (HttpRequestException ex)
    {
        return (null, $"Network error: {ex.Message}");
    }
    catch (Exception ex)
    {
        return (null, $"Unexpected error: {ex.Message}");
    }
}

}
