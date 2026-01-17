using AutoMapper;
using CleanArchitecture.Application.DTOs.Product;
using CleanArchitecture.Application.Mappings;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace CleanArchitecture.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);

        _productService = new ProductService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Electronics",
            StockQuantity = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999;

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _productService.GetByIdAsync(productId));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.00m, Category = "Cat1", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Product 2", Price = 20.00m, Category = "Cat2", CreatedAt = DateTime.UtcNow }
        };

        _productRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "New Description",
            Price = 49.99m,
            Category = "Electronics",
            StockQuantity = 25
        };

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) =>
            {
                p.Id = 1;
                return p;
            });

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _productService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(49.99m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedProduct()
    {
        // Arrange
        var productId = 1;
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Original Name",
            Description = "Original Description",
            Price = 10.00m,
            Category = "Original Category",
            StockQuantity = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 15.00m,
            Category = "Updated Category",
            StockQuantity = 10,
            IsActive = true
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _productService.UpdateAsync(productId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Price.Should().Be(15.00m);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            CreatedAt = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _productService.DeleteAsync(productId);

        // Assert
        _productRepositoryMock.Verify(r => r.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999;

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _productService.DeleteAsync(productId));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResult()
    {
        // Arrange
        var filter = new ProductFilterDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.00m, Category = "Cat1", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Product 2", Price = 20.00m, Category = "Cat2", CreatedAt = DateTime.UtcNow }
        };

        _productRepositoryMock
            .Setup(r => r.GetPagedAsync(
                filter.PageNumber,
                filter.PageSize,
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _productService.GetPagedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsProductsInCategory()
    {
        // Arrange
        var category = "Electronics";
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Category = category, Price = 999.99m, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Mouse", Category = category, Price = 29.99m, CreatedAt = DateTime.UtcNow }
        };

        _productRepositoryMock
            .Setup(r => r.GetByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetByCategoryAsync(category);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Category.Should().Be(category));
    }
}
