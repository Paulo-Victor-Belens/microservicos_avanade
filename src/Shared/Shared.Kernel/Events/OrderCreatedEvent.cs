// Este é o contrato da mensagem que será enviada para o RabbitMQ
namespace Shared.Kernel.Events
{
    public record OrderCreatedEvent(long OrderId, List<OrderItemEventDto> Items);
    
    public record OrderItemEventDto(long ProductId, int Quantity);
}