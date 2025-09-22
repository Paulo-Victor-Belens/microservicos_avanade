namespace Shared.Kernel.Events
{
    public record OrderCreatedEvent(long OrderId, List<OrderItemEventDto> Items);
    
    public record OrderItemEventDto(long ProductId, int Quantity);
}