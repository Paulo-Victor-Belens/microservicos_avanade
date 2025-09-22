namespace Order.Api.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Model.Order order);
        Task<Model.Order?> GetByIdAsync(long id);
        void Update(Model.Order order);
        Task<bool> SaveChangesAsync();
    }
}