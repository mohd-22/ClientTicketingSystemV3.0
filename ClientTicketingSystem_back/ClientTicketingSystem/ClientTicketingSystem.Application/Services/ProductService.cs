using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;
using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;
using ClientTicketingSystem.CORE.Specifications;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;

namespace ClientTicketingSystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<ProductDto>> GetProductById(Guid productId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetProductWithItemsAsync(productId);

            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", productId);
                return new ApiResponse<ProductDto> { Success = false, Message = "Product not found", StatusCode = 404 };
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Modules = product.ProductModules!.Select(i => new ModuleDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description
                }).ToList()
            };

            _logger.LogInformation("Retrieved product {ProductId} with {ModuleCount} modules", productId, productDto.Modules.Count);
            return new ApiResponse<ProductDto> { Success = true, Message = "Product Retrieved Succesfully", Data = productDto, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", productId);
            return new ApiResponse<ProductDto> { Success = false, Message = "An error occurred while retrieving the product", StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<PaginationDto<ProductWithCountDto>>> GetAllProducts(string? search, string? sort, int pageIndex, int pageSize)
    {
        try
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var spec = new ProductsWithFiltersSpecification(search, sort, pageIndex, pageSize);
            var countSpec = new ProductsWithFiltersForCountSpecification(search);

            var products = await _unitOfWork.Products.GetProductsWithModulesCountAsync(spec);
            var totalCount = await _unitOfWork.Products.CountAsync(countSpec);

            var pagedResult = new PaginationDto<ProductWithCountDto>(pageIndex, pageSize, totalCount, products);

            _logger.LogInformation(
                "Retrieved {ProductCount} of {TotalCount} products (page {PageIndex}, size {PageSize})",
                products.Count, totalCount, pageIndex, pageSize);

            return new ApiResponse<PaginationDto<ProductWithCountDto>>
            {
                Success = true,
                Message = "Products retrieved successfully",
                Data = pagedResult,
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return new ApiResponse<PaginationDto<ProductWithCountDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving products",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<CreateProductDto>> CreateProduct(CreateProductDto catDto, Guid userId)
    {
        try
        {
            if (await _unitOfWork.Products.AnyAsync(x => x.Name == catDto.Name))
            {
                _logger.LogWarning("Create product failed: duplicate name {ProductName}", catDto.Name);
                return new ApiResponse<CreateProductDto> { Success = false, Message = "Product with the same name already exists", StatusCode = 400 };
            }

            var newProduct = new Product
            {
                Name = catDto.Name,
                Description = catDto.Description,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.Products.AddAsync(newProduct);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product {ProductId} created by user {UserId}", newProduct.Id, userId);
            return new ApiResponse<CreateProductDto> { Success = true, Message = "Products Created Succesfully", Data = catDto, StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName} for user {UserId}", catDto.Name, userId);
            return new ApiResponse<CreateProductDto> { Success = false, Message = "An error occurred while creating the product", StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<UpdateProductDto>> UpdateProduct(UpdateProductDto catDto, Guid userId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(catDto.Id);
            if (product == null)
            {
                _logger.LogWarning("Update product failed: product {ProductId} not found", catDto.Id);
                return new ApiResponse<UpdateProductDto> { Success = false, Message = "Product not found", StatusCode = 404 };
            }

            product.Name = catDto.Name;
            product.Description = catDto.Description;
            product.LastUpdatedDate = DateTime.UtcNow;
            product.LastUpdatedBy = userId;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product {ProductId} updated by user {UserId}", catDto.Id, userId);
            return new ApiResponse<UpdateProductDto> { Success = true, Message = "Products Retrieved Succesfully", Data = catDto, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId} for user {UserId}", catDto.Id, userId);
            return new ApiResponse<UpdateProductDto> { Success = false, Message = "An error occurred while updating the product", StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<bool>> DeleteProduct(Guid id)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Delete product failed: product {ProductId} not found", id);
                return new ApiResponse<bool> { Success = false, Message = "Product not found", StatusCode = 404 };
            }

            var hasLinkedRequests = await _unitOfWork.Tickets.AnyAsync(r =>
                r.ProductModule!.ProductId == id &&
                r.Status != TicketStatus.Closed);

            if (hasLinkedRequests)
            {
                _logger.LogWarning("Delete product blocked: open tickets exist for product {ProductId}", id);
                return new ApiResponse<bool> { Success = false, Message = "There Are Tickets associated with this Product!", StatusCode = 400 };
            }

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product {ProductId} deleted", id);
            return new ApiResponse<bool> { Success = true, Message = "Product Deleted Successfully", Data = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return new ApiResponse<bool> { Success = false, Message = "An error occurred while deleting the product", StatusCode = 500 };
        }
    }
}
