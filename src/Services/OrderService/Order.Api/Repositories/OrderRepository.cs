using Microsoft.EntityFrameworkCore;
using Order.Api.Data;

namespace Order.Api.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Model.Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task<Model.Order?> GetByIdAsync(long id)
        {
            return await _context.Orders
                                .Include(o => o.Items)
                                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public void Update(Model.Order order)
        {
            _context.Orders.Update(order);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}