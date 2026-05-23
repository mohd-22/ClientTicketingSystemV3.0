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
    public async Task<ApiResponse<PaginationDto<ProductWithCountDto>>> GetAllProducts(string? search,
                                                                                      string? sort,
                                                                                      int pageIndex,
                                                                                      int pageSize)
    {
        try
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var spec = new ProductsWithFiltersSpecification(search, sort, pageIndex, pageSize);
            var countSpec = new ProductsWithFiltersForCountSpecification(search);

            var products = await _unitOfWork.Products.GetProductsAsync(spec);
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
}
