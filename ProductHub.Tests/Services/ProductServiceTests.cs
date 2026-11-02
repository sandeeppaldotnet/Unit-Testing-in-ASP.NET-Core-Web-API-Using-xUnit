using Moq;
using ProductHub.API.Models;
using ProductHub.API.Repositories;
using ProductHub.API.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductHub.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;

        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _repositoryMock = new Mock<IProductRepository>();

            _productService = new ProductService(
                _repositoryMock.Object);
        }

        // 1. Valid discount calculation
        [Fact]
        public void CalculateDiscount_ValidInput_ReturnsDiscountedPrice()
        {
            // Arrange
            decimal price = 1000m;
            decimal discount = 10m;
            decimal expected = 900m;

            // Act
            var actual = _productService.CalculateDiscount(
                price, discount);

            // Assert
            Assert.Equal(expected, actual);
        }

        // 2. Multiple discount scenarios
        [Theory]
        [InlineData(1000, 10, 900)]
        [InlineData(1000, 20, 800)]
        [InlineData(1000, 30, 700)]
        [InlineData(2000, 50, 1000)]
        [InlineData(500, 0, 500)]
        public void CalculateDiscount_MultipleInputs_ReturnsExpectedPrice(
            decimal price,
            decimal discount,
            decimal expected)
        {
            // Arrange

            // Act
            var actual = _productService.CalculateDiscount(
                price, discount);

            // Assert
            Assert.Equal(expected, actual);
        }

        // 3. Invalid discount
        [Theory]
        [InlineData(150)]
        [InlineData(-10)]
        public void CalculateDiscount_InvalidDiscount_ThrowsException(
            decimal discount)
        {
            // Arrange
            decimal price = 1000m;

            // Act
            var exception = Assert.Throws<ArgumentException>(
                () => _productService.CalculateDiscount(
                    price, discount));

            // Assert
            Assert.Contains(
                "Discount must be between 0 and 100",
                exception.Message);
        }


        // 4. Invalid product ID
        [Fact]
        public async Task GetByIdAsync_InvalidId_ThrowsException()
        {
            // Arrange
            int productId = 0;

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.GetByIdAsync(productId));

            // Assert
            Assert.Contains(
                "Product ID must be greater than zero",
                exception.Message);
        }


        // 5. Retrieve existing product
        [Fact]
        public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            var expectedProduct = new Product
            {
                Id = 1,
                Name = "Mechanical Keyboard",
                Price = 2500,
                Stock = 20
            };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(expectedProduct);

            // Act
            var actualProduct = await _productService.GetByIdAsync(1);

            // Assert
            Assert.NotNull(actualProduct);
            Assert.Equal(expectedProduct.Id, actualProduct.Id);
            Assert.Equal(expectedProduct.Name, actualProduct.Name);
            Assert.Equal(expectedProduct.Price, actualProduct.Price);
        }


        // 6. Product not found
        [Fact]
        public async Task GetByIdAsync_ProductNotFound_ReturnsNull()
        {
            // Arrange
            int productId = 100;

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.Null(result);
        }


        // 7. Successful product creation
        [Fact]
        public async Task CreateAsync_ValidProduct_ReturnsProduct()
        {
            // Arrange
            var product = new Product
            {
                Name = "Gaming Headset",
                Price = 3500,
                Stock = 15
            };

            _repositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.CreateAsync(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Gaming Headset", result.Name);
            Assert.Equal(3500m, result.Price);

            _repositoryMock.Verify(
                repo => repo.AddAsync(It.IsAny<Product>()),
                Times.Once);
        }

        // 8. Empty product name
        [Fact]
        public async Task CreateAsync_EmptyName_ThrowsException()
        {
            // Arrange
            var product = new Product
            {
                Name = "",
                Price = 1000,
                Stock = 10
            };

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.CreateAsync(product));

            // Assert
            Assert.Contains(
                "Product name is required",
                exception.Message);

            _repositoryMock.Verify(
                repo => repo.AddAsync(It.IsAny<Product>()),
                Times.Never);
        }

        // 9. Invalid product price
        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public async Task CreateAsync_InvalidPrice_ThrowsException(
            decimal price)
        {
            // Arrange
            var product = new Product
            {
                Name = "Gaming Mouse",
                Price = price,
                Stock = 10
            };

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.CreateAsync(product));

            // Assert
            Assert.Contains(
                "Product price must be greater than zero",
                exception.Message);

            _repositoryMock.Verify(
                repo => repo.AddAsync(It.IsAny<Product>()),
                Times.Never);
        }

        // 10. Negative stock
        [Fact]
        public async Task CreateAsync_NegativeStock_ThrowsException()
        {
            // Arrange
            var product = new Product
            {
                Name = "Gaming Mouse",
                Price = 1500,
                Stock = -5
            };

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.CreateAsync(product));

            // Assert
            Assert.Contains(
                "Product stock cannot be negative",
                exception.Message);

            _repositoryMock.Verify(
                repo => repo.AddAsync(It.IsAny<Product>()),
                Times.Never);
        }
    }


}
