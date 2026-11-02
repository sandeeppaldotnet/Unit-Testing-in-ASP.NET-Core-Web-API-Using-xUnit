using ProductHub.API.Models;
using ProductHub.API.Repositories;

namespace ProductHub.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Product ID must be greater than zero.");
            }

            return await _repository.GetByIdAsync(id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException(
                    "Product name is required.");
            }

            if (product.Price <= 0)
            {
                throw new ArgumentException(
                    "Product price must be greater than zero.");
            }

            if (product.Stock < 0)
            {
                throw new ArgumentException(
                    "Product stock cannot be negative.");
            }

            await _repository.AddAsync(product);

            return product;
        }

        public decimal CalculateDiscount(
            decimal price,
            decimal discountPercentage)
        {
            if (price <= 0)
            {
                throw new ArgumentException(
                    "Price must be greater than zero.");
            }

            if (discountPercentage < 0 ||
                discountPercentage > 100)
            {
                throw new ArgumentException(
                    "Discount must be between 0 and 100.");
            }

            return price - (price * discountPercentage / 100);
        }
    }

}
