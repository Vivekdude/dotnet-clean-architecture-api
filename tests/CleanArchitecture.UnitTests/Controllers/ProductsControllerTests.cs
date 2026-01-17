using CleanArchitecture.API.Controllers;
using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.DTOs.Product;
using CleanArchitecture.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanArchitecture.UnitTests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_productServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResultWithPagedProducts()
    {
        // Arrange
        var filter = new ProductFilterDto { PageNumber = 1, PageSize = 10 };
        var products = new List<ProductDto>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.00m },
            new() { Id = 2, Name = "Product 2", Price = 20.00m }
        };
        var pagedResult = new PagedResult<ProductDto>(products, 2, 1, 10);

        _productServiceMock
            .Setup(s => s.GetPagedAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(filter, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PagedResult<ProductDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResultWithProduct()
    {
        // Arrange
        var productId = 1;
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 99.99m
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(productId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<ProductDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(productId);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Price = 49.99m,
            Category = "Electronics",
            StockQuantity = 25
        };

        var createdProduct = new ProductDto
        {
            Id = 1,
            Name = createDto.Name,
            Price = createDto.Price,
            Category = createDto.Category,
            StockQuantity = createDto.StockQuantity,
            IsActive = true
        };

        _productServiceMock
            .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(createDto, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<ApiResponse<ProductDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Name.Should().Be("New Product");
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var productId = 1;
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Price = 59.99m,
            Category = "Electronics",
            StockQuantity = 30,
            IsActive = true
        };

        var updatedProduct = new ProductDto
        {
            Id = productId,
            Name = updateDto.Name,
            Price = updateDto.Price,
            Category = updateDto.Category,
            StockQuantity = updateDto.StockQuantity,
            IsActive = updateDto.IsActive
        };

        _productServiceMock
            .Setup(s => s.UpdateAsync(productId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(productId, updateDto, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<ProductDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Name.Should().Be("Updated Product");
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var productId = 1;

        _productServiceMock
            .Setup(s => s.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(productId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Contain("deleted");
    }

    [Fact]
    public async Task GetByCategory_ReturnsOkResultWithProducts()
    {
        // Arrange
        var category = "Electronics";
        var products = new List<ProductDto>
        {
            new() { Id = 1, Name = "Laptop", Category = category, Price = 999.99m },
            new() { Id = 2, Name = "Mouse", Category = category, Price = 29.99m }
        };

        _productServiceMock
            .Setup(s => s.GetByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetByCategory(category, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<ProductDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
    }
}
