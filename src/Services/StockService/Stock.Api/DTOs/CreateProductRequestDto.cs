namespace Stock.Api.DTOs
{
    public record CreateProductRequestDto(
        string Sku, 
        string Name, 
        string Description, 
        string Category, 
        decimal Price, 
        int Quantity, 
        string? ImageUrl
    );
}