namespace Stock.Api.DTOs
{
    public record UpdateProductRequestDto(
        string Name,
        string Description,
        string Category,
        decimal Price,
        string? ImageUrl
    );
}