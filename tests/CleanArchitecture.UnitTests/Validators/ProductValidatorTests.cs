using CleanArchitecture.Application.DTOs.Product;
using CleanArchitecture.Application.Validators;
using FluentAssertions;

namespace CleanArchitecture.UnitTests.Validators;

public class ProductValidatorTests
{
    private readonly CreateProductValidator _createValidator;
    private readonly UpdateProductValidator _updateValidator;

    public ProductValidatorTests()
    {
        _createValidator = new CreateProductValidator();
        _updateValidator = new UpdateProductValidator();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Description = "A valid product description",
            Price = 99.99m,
            Category = "Electronics",
            StockQuantity = 10
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "",
            Price = 99.99m,
            Category = "Electronics"
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Price = -10.00m,
            Category = "Electronics"
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task CreateProduct_WithZeroPrice_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Price = 0m,
            Category = "Electronics"
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task CreateProduct_WithEmptyCategory_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Price = 99.99m,
            Category = ""
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Category");
    }

    [Fact]
    public async Task CreateProduct_WithNegativeStockQuantity_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "Valid Product",
            Price = 99.99m,
            Category = "Electronics",
            StockQuantity = -5
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StockQuantity");
    }

    [Fact]
    public async Task CreateProduct_WithNameExceedingMaxLength_FailsValidation()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = new string('A', 201), // Max is 200
            Price = 99.99m,
            Category = "Electronics"
        };

        // Act
        var result = await _createValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "An updated description",
            Price = 149.99m,
            Category = "Electronics",
            StockQuantity = 20,
            IsActive = true
        };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProduct_WithMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var dto = new UpdateProductDto
        {
            Name = "",
            Price = -10.00m,
            Category = "",
            StockQuantity = -5
        };

        // Act
        var result = await _updateValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }
}
