namespace Order.Api.DTOs
{
    public record CreateOrderRequestDto(long CustomerId, List<OrderItemDto> Items);
    
    public record OrderItemDto(long ProductId, int Quantity, decimal UnitPrice);
}