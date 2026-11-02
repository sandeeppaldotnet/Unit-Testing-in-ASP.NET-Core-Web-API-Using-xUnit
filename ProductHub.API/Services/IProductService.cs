using ProductHub.API.Models;

namespace ProductHub.API.Services
{
    public interface IProductService
    {
        Task<Product?> GetByIdAsync(int id);

        Task<Product> CreateAsync(Product product);

        decimal CalculateDiscount(
            decimal price,
            decimal discountPercentage);
    }
}
