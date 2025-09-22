using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Api.DTOs;
using Order.Api.Services;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var result = await _orderService.CreateOrderAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return CreatedAtAction(nameof(GetOrderById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> GetOrderById(long id)
        {
            var result = await _orderService.GetByIdAsync(id);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return NotFound(new { message = result.Error });
        }
    }
}