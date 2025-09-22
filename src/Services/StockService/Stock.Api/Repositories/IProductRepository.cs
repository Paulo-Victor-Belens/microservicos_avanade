using Stock.Api.Model;

namespace Stock.Api.Repositories
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task<Product?> GetByIdAsync(long id);
        Task<IEnumerable<Product>> GetAllAsync();
        void Update(Product product);
        Task<bool> SaveChangesAsync();
    }
}