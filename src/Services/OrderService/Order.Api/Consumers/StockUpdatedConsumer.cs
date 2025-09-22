using MassTransit;
using Order.Api.Repositories;
using Shared.Kernel.Events;
using System.Threading.Tasks;

namespace Order.Api.Consumers
{
    public class StockUpdatedConsumer : IConsumer<StockUpdatedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<StockUpdatedConsumer> _logger;

        public StockUpdatedConsumer(IOrderRepository orderRepository, ILogger<StockUpdatedConsumer> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockUpdatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("==> Mensagem de confirmação StockUpdatedEvent recebida para o pedido: {OrderId}", message.OrderId);

            var order = await _orderRepository.GetByIdAsync(message.OrderId);

            if (order != null)
            {
                order.ConfirmOrder();
                await _orderRepository.SaveChangesAsync();
                _logger.LogInformation("Status do pedido {OrderId} atualizado para Confirmado.", order.Id);
            }
            else
            {
                _logger.LogWarning("Pedido com Id {OrderId} não encontrado para confirmar.", message.OrderId);
            }
        }
    }
}