using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;
using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Specifications;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;

namespace ClientTicketingSystem.Application.Services;

public class ProductModuleService : IProductModuleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductModuleService> _logger;

    public ProductModuleService(IUnitOfWork unitOfWork, ILogger<ProductModuleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<CreateModuleDto>> CreateModuleAsync(CreateModuleDto dto, Guid userId)
    {
        try
        {
            var productExists = await _unitOfWork.Products.GetByIdAsync(dto.ProdutId);
            if (productExists == null)
            {
                _logger.LogWarning("Create module failed: product {ProductId} not found", dto.ProdutId);
                return new ApiResponse<CreateModuleDto> { Success = false, Message = "Product not found", StatusCode = 404 };
            }

            var module = new ProductModule
            {
                Name = dto.Name,
                Description = dto.Description,
                ProductId = dto.ProdutId,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.ProdectModules.AddAsync(module);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Module {ModuleId} created for product {ProductId} by user {UserId}",
                module.Id, dto.ProdutId, userId);
            return new ApiResponse<CreateModuleDto> { Success = true, Data = dto, Message = "Product Created Successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating module for product {ProductId}", dto.ProdutId);
            return new ApiResponse<CreateModuleDto> { Success = false, Message = "An error occurred while creating the module", StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<ModuleDto>> GetModuleByIdAsync(Guid id)
    {
        try
        {
            var module = await _unitOfWork.ProdectModules.GetByIdAsync(id);
            if (module == null)
            {
                _logger.LogWarning("Module {ModuleId} not found", id);
                return new ApiResponse<ModuleDto> { Success = false, Message = "Module not found", StatusCode = 404 };
            }

            var moduleDto = new ModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                Description = module.Description,
                ProdutId = module.ProductId
            };

            return new ApiResponse<ModuleDto> { Success = true, Message = "Module Retrieved Successfully", Data = moduleDto, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module {ModuleId}", id);
            return new ApiResponse<ModuleDto> { Success = false, Message = "An error occurred while retrieving the module", StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<PaginationDto<ModuleDto>>> GetAllModulesAsync(
        string? search, string? sort, Guid? productId, int pageIndex, int pageSize)
    {
        try
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var spec = new ModulesWithFiltersSpecification(search, sort, productId, pageIndex, pageSize);
            var countSpec = new ModulesWithFiltersForCountSpecification(search, productId);

            var modules = await _unitOfWork.ProdectModules.ListWithSpecAsync(spec);
            var totalCount = await _unitOfWork.ProdectModules.CountAsync(countSpec);

            var moduleDtos = modules.Select(module => new ModuleDto
            {
                Id = module.Id,
                Name = module.Name,
                Description = module.Description,
                ProdutId = module.ProductId
            }).ToList();

            var pagedResult = new PaginationDto<ModuleDto>(pageIndex, pageSize, totalCount, moduleDtos);

            _logger.LogInformation(
                "Retrieved {ModuleCount} of {TotalCount} modules (page {PageIndex}, size {PageSize})",
                moduleDtos.Count, totalCount, pageIndex, pageSize);

            return new ApiResponse<PaginationDto<ModuleDto>>
            {
                Success = true,
                Message = "Modules retrieved successfully",
                Data = pagedResult,
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving modules");
            return new ApiResponse<PaginationDto<ModuleDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving modules",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<UpdateProductDto>> UpdateModule(UpdateProductDto moduleDto, Guid userId)
    {
        try
        {
            var module = await _unitOfWork.ProdectModules.GetByIdAsync(moduleDto.Id);
            if (module == null)
            {
                _logger.LogWarning("Update module failed: module {ModuleId} not found", moduleDto.Id);
                return new ApiResponse<UpdateProductDto> { Success = false, Message = "Module not found", StatusCode = 404 };
            }

            module.Id = moduleDto.Id;
            module.Name = moduleDto.Name;
            module.Description = moduleDto.Description;
            module.LastUpdatedBy = userId;
            module.LastUpdatedDate = DateTime.UtcNow;

            _unitOfWork.ProdectModules.Update(module);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Module {ModuleId} updated by user {UserId}", moduleDto.Id, userId);
            return new ApiResponse<UpdateProductDto> { Success = true, Message = "Module Updated Successfully", Data = moduleDto, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module {ModuleId} for user {UserId}", moduleDto.Id, userId);
            return new ApiResponse<UpdateProductDto> { Success = false, Message = "An error occurred while updating the module", StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<bool>> DeleteModule(Guid id)
    {
        try
        {
            var module = await _unitOfWork.ProdectModules.GetByIdAsync(id);
            if (module == null)
            {
                _logger.LogWarning("Delete module failed: module {ModuleId} not found", id);
                return new ApiResponse<bool> { Success = false, Message = "Module not found", StatusCode = 404 };
            }

            var hasLinkedTickets = await _unitOfWork.Tickets.AnyAsync(x => x.ProductMoudleId == id);
            if (hasLinkedTickets)
            {
                _logger.LogWarning("Delete module blocked: tickets linked to module {ModuleId}", id);
                return new ApiResponse<bool> { Success = false, Message = "Cannot delete module with linked tickets", StatusCode = 400 };
            }

            _unitOfWork.ProdectModules.Delete(module);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Module {ModuleId} deleted", id);
            return new ApiResponse<bool> { Success = true, Message = "Module Deleted Successfully", Data = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module {ModuleId}", id);
            return new ApiResponse<bool> { Success = false, Message = "An error occurred while deleting the module", StatusCode = 500 };
        }
    }
}
