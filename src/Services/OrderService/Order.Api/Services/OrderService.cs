using IdGen;
using MassTransit;
using Order.Api.DTOs;
using Order.Api.Model;
using Order.Api.Repositories;
using Shared.Kernel.Events;
using Shared.Kernel.Core;
using IdentityModel.Client;

namespace Order.Api.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderService> _logger;
        private readonly IdGenerator _idGenerator;

        public OrderService(
            IOrderRepository orderRepository,
            IPublishEndpoint publishEndpoint,
            IHttpClientFactory httpClientFactory,
            ILogger<OrderService> logger,
            IdGenerator idGenerator)
        {
            _orderRepository = orderRepository;
            _publishEndpoint = publishEndpoint;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _idGenerator = idGenerator;
        }

        public async Task<Result<Model.Order>> CreateOrderAsync(CreateOrderRequestDto request)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var discoveryDoc = await httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = "http://identity-api:8080",
                Policy =
                {
                    RequireHttps = false
                }
            });

            if (discoveryDoc.IsError)
            {
                _logger.LogError(discoveryDoc.Error, "Não foi possível encontrar o serviço de identidade.");
                return Result<Model.Order>.Failure("Não foi possível encontrar o serviço de identidade.");
            }

            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,
                ClientId = "order.service",
                ClientSecret = "secret",
                Scope = "stock.api"
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError(tokenResponse.Error, "Não foi possível obter um token para o serviço de estoque.");
                return Result<Model.Order>.Failure("Não foi possível obter um token para o serviço de estoque.");
            }
            
            httpClient.SetBearerToken(tokenResponse.AccessToken!);
            

            foreach (var item in request.Items)
            {
                var requestUrl = $"http://stock-api:8080/api/products/{item.ProductId}/validate?quantity={item.Quantity}&price={item.UnitPrice}";
                
                try
                {
                    var response = await httpClient.GetAsync(requestUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Falha na validação de estoque/preço: {StatusCode} - {Content}", response.StatusCode, errorContent);
                        return Result<Model.Order>.Failure($"Não foi possível validar o item {item.ProductId}. Motivo: {errorContent}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Erro de comunicação ao tentar validar o estoque para o produto {ProductId}", item.ProductId);
                    return Result<Model.Order>.Failure("Não foi possível se comunicar com o serviço de estoque. Tente novamente mais tarde.");
                }
            }

            var orderItems = request.Items.Select(item => new OrderItem(_idGenerator.CreateId(), item.ProductId, item.Quantity, item.UnitPrice)).ToList();
            var newOrder = new Model.Order(_idGenerator.CreateId(), request.CustomerId, orderItems);
            
            await _orderRepository.AddAsync(newOrder);
            await _orderRepository.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvent(newOrder.Id, newOrder.Items.Select(i => new OrderItemEventDto(i.ProductId, i.Quantity)).ToList());
            await _publishEndpoint.Publish(orderCreatedEvent);
            
            return Result<Model.Order>.Success(newOrder);
        }

        public async Task<Result<Model.Order>> GetByIdAsync(long id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order != null)
            {
                return Result<Model.Order>.Success(order);
            }
            
            return Result<Model.Order>.Failure("Pedido não encontrado.");
        }
    }
}