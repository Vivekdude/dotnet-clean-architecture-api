using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.DTOs.Product;
using CleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with pagination and filtering
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all products", Description = "Retrieve a paginated list of products with optional filtering")]
    [SwaggerResponse(200, "Products retrieved successfully", typeof(ApiResponse<PagedResult<ProductDto>>))]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetAll(
        [FromQuery] ProductFilterDto filter,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetPagedAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResult<ProductDto>>.SuccessResponse(result, "Products retrieved successfully"));
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get product by ID", Description = "Retrieve a single product by its ID")]
    [SwaggerResponse(200, "Product retrieved successfully", typeof(ApiResponse<ProductDto>))]
    [SwaggerResponse(404, "Product not found")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(product!, "Product retrieved successfully"));
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    [SwaggerOperation(Summary = "Get products by category", Description = "Retrieve all products in a specific category")]
    [SwaggerResponse(200, "Products retrieved successfully", typeof(ApiResponse<IEnumerable<ProductDto>>))]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetByCategory(
        string category,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetByCategoryAsync(category, cancellationToken);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(products, "Products retrieved successfully"));
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a product", Description = "Create a new product")]
    [SwaggerResponse(201, "Product created successfully", typeof(ApiResponse<ProductDto>))]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(
        [FromBody] CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            ApiResponse<ProductDto>.SuccessResponse(product, "Product created successfully"));
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:int}")]
    [SwaggerOperation(Summary = "Update a product", Description = "Update an existing product by ID")]
    [SwaggerResponse(200, "Product updated successfully", typeof(ApiResponse<ProductDto>))]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(
        int id,
        [FromBody] UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<ProductDto>.SuccessResponse(product, "Product updated successfully"));
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Delete a product", Description = "Delete a product by ID")]
    [SwaggerResponse(200, "Product deleted successfully")]
    [SwaggerResponse(404, "Product not found")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Product deleted successfully"));
    }
}
