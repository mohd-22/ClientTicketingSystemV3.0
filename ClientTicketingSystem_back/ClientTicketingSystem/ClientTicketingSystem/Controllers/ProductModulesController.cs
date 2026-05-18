using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientTicketingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductModulesController : ControllerBase
{
    private readonly IProductModuleService _moduleService;
    private readonly ILogger<ProductModulesController> _logger;

    public ProductModulesController(IProductModuleService moduleService, ILogger<ProductModulesController> logger)
    {
        _moduleService = moduleService;
        _logger = logger;
    }

    [Authorize(Roles = $"{nameof(UserRole.Manager)}")]
    [HttpPost("CreateModule")]
    public async Task<ActionResult> CreateModule(CreateModuleDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            _logger.LogWarning("CreateModule rejected: invalid or missing user id claim");
            return Unauthorized();
        }

        _logger.LogInformation("CreateModule requested by {UserId} for product {ProductId}", userGuid, dto.ProdutId);
        var result = await _moduleService.CreateModuleAsync(dto, userGuid);

        if (!result.Success)
            _logger.LogWarning("CreateModule failed for product {ProductId}. Status: {StatusCode}, Message: {Message}",
                dto.ProdutId, result.StatusCode, result.Message);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("GetModule/{Id}")]
    public async Task<ActionResult> GetModule(Guid Id)
    {
        _logger.LogInformation("GetModule requested for {ModuleId}", Id);
        var result = await _moduleService.GetModuleByIdAsync(Id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("GetAllModules")]
    public async Task<ActionResult> GetAllModules(  
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] Guid? productId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(
            "GetAllModules requested with search={Search}, sort={Sort}, productId={ProductId}, pageIndex={PageIndex}, pageSize={PageSize}",
            search, sort, productId, pageIndex, pageSize);

        var result = await _moduleService.GetAllModulesAsync(search, sort, productId, pageIndex, pageSize);

        _logger.LogDebug("GetAllModules returned status {StatusCode}", result.StatusCode);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = $"{nameof(UserRole.Manager)}")]
    [HttpPost("UpdateModules")]
    public async Task<ActionResult> UpdateModule(UpdateProductDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            _logger.LogWarning("UpdateModule rejected: invalid or missing user id claim");
            return Unauthorized();
        }

        _logger.LogInformation("UpdateModule requested by {UserId} for module {ModuleId}", userGuid, dto.Id);
        var result = await _moduleService.UpdateModule(dto, userGuid);

        if (!result.Success)
            _logger.LogWarning("UpdateModule failed for {ModuleId}. Status: {StatusCode}, Message: {Message}",
                dto.Id, result.StatusCode, result.Message);

        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{Id}")]
    public async Task<ActionResult> DeleteModule(Guid Id)
    {
        _logger.LogInformation("DeleteModule requested for {ModuleId}", Id);
        var result = await _moduleService.DeleteModule(Id);

        if (!result.Success)
            _logger.LogWarning("DeleteModule failed for {ModuleId}. Status: {StatusCode}, Message: {Message}",
                Id, result.StatusCode, result.Message);
        else
            _logger.LogInformation("DeleteModule succeeded for {ModuleId}", Id);

        return StatusCode(result.StatusCode, result);
    }
}
