using ProductHub.API.Models;

namespace ProductHub.API.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);

        Task AddAsync(Product product);
    }

}
