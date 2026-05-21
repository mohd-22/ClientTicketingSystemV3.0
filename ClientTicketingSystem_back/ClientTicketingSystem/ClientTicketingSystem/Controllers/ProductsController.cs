using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace ClientTicketingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    public ProductsController(IProductService productService,
                              ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet("GetProductById/{id}")]
    public async Task<ActionResult> GetProductById(Guid id)
    {
        var result = await _productService.GetProductById(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("GetAllProducts")]
    public async Task<ActionResult> GetAllProducts(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _productService.GetAllProducts(search, sort, pageIndex, pageSize);
        return StatusCode(result.StatusCode, result);

    }

    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpPost("CreateProduct")]
    public async Task<ActionResult> CreateProduct(CreateProductDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return Unauthorized();
        }
        var result = await _productService.CreateProduct(dto, userGuid);
        return StatusCode(result.StatusCode, result);

    }

    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpPost("UpdateProduct")]
    public async Task<ActionResult> UpdateProduct(UpdateProductDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return Unauthorized();
        }
        var result = await _productService.UpdateProduct(dto, userGuid);
        return StatusCode(result.StatusCode, result);
    }


    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpDelete("{Id}")]
    public async Task<ActionResult> DeleteItem(Guid Id)
    {
        var result = await _productService.DeleteProduct(Id);
        return StatusCode(result.StatusCode, result);
    }

}


