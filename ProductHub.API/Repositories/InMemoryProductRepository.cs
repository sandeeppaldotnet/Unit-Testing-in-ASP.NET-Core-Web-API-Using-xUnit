using ProductHub.API.Models;

namespace ProductHub.API.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new()
    {
        new Product
        {
            Id = 1,
            Name = "Mechanical Keyboard",
            Price = 2500,
            Stock = 20
        },
        new Product
        {
            Id = 2,
            Name = "Wireless Mouse",
            Price = 1200,
            Stock = 35
        }
    };

        public Task<Product?> GetByIdAsync(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);

            return Task.FromResult(product);
        }

        public Task AddAsync(Product product)
        {
            product.Id = _products.Count + 1;

            _products.Add(product);

            return Task.CompletedTask;
        }
    }


}
