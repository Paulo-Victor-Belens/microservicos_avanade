using MassTransit;
using Shared.Kernel.Events;
using Stock.Api.Repositories;

namespace Stock.Api.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderCreatedConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedConsumer(IProductRepository productRepository, ILogger<OrderCreatedConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _productRepository = productRepository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("==> Mensagem OrderCreatedEvent recebida para o pedido: {OrderId}", message.OrderId);

            foreach (var item in message.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);

                if (product != null)
                {
                    product.DecreaseStock(item.Quantity);
                    _productRepository.Update(product);
                    _logger.LogInformation("Estoque do produto {ProductId} atualizado. Nova quantidade: {Quantity}", product.Id, product.Quantity);
                }
                else
                {
                    _logger.LogWarning("Produto com Id {ProductId} não encontrado no estoque.", item.ProductId);
                }
            }

            await _productRepository.SaveChangesAsync();
            _logger.LogInformation("Estoque atualizado com sucesso para o pedido {OrderId}.", message.OrderId);

            var stockUpdatedEvent = new StockUpdatedEvent(message.OrderId);
            await _publishEndpoint.Publish(stockUpdatedEvent);

            _logger.LogInformation("==> Evento de confirmação StockUpdatedEvent publicado para o pedido: {OrderId}", message.OrderId);
        }
    }
}