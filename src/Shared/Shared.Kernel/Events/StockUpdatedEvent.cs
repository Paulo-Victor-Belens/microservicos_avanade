namespace Shared.Kernel.Events
{
    // O evento de confirmação que o StockService publica de volta
    public record StockUpdatedEvent(long OrderId);
}