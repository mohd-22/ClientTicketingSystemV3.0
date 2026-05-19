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



    public ProductsController(IProductService productService, ILogger<ProductsController> logger)

    {

        _productService = productService;

        _logger = logger;

    }



    [HttpGet("GetProductById/{id}")]

    public async Task<ActionResult> GetProductById(Guid id)

    {

        _logger.LogInformation("GetProductById requested for {ProductId}", id);

        var result = await _productService.GetProductById(id);

        _logger.LogDebug("GetProductById {ProductId} returned status {StatusCode}", id, result.StatusCode);

        return StatusCode(result.StatusCode, result);

    }



    [HttpGet("GetAllProducts")]

    public async Task<ActionResult> GetAllProducts(

        [FromQuery] string? search,

        [FromQuery] string? sort,

        [FromQuery] int pageIndex = 1,

        [FromQuery] int pageSize = 10)

    {

        _logger.LogInformation(

            "GetAllProducts requested with search={Search}, sort={Sort}, pageIndex={PageIndex}, pageSize={PageSize}",

            search, sort, pageIndex, pageSize);



        var result = await _productService.GetAllProducts(search, sort, pageIndex, pageSize);



        _logger.LogDebug("GetAllProducts returned status {StatusCode}", result.StatusCode);

        return StatusCode(result.StatusCode, result);

    }



    [Authorize(Roles = nameof(UserRole.Manager))]

    [HttpPost("CreateProduct")]

    public async Task<ActionResult> CreateProduct(CreateProductDto dto)

    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out Guid userGuid))

        {

            _logger.LogWarning("CreateProduct rejected: invalid or missing user id claim");

            return Unauthorized();

        }



        _logger.LogInformation("CreateProduct requested by {UserId} for product {ProductName}", userGuid, dto.Name);

        var result = await _productService.CreateProduct(dto, userGuid);



        if (!result.Success)

            _logger.LogWarning("CreateProduct failed for {ProductName}. Status: {StatusCode}, Message: {Message}",

                dto.Name, result.StatusCode, result.Message);



        return StatusCode(result.StatusCode, result);

    }



    [Authorize(Roles = nameof(UserRole.Manager))]

    [HttpPost("UpdateProduct")]

    public async Task<ActionResult> UpdateProduct(UpdateProductDto dto)

    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out Guid userGuid))

        {

            _logger.LogWarning("UpdateProduct rejected: invalid or missing user id claim");

            return Unauthorized();

        }



        _logger.LogInformation("UpdateProduct requested by {UserId} for product {ProductId}", userGuid, dto.Id);

        var result = await _productService.UpdateProduct(dto, userGuid);



        if (!result.Success)

            _logger.LogWarning("UpdateProduct failed for {ProductId}. Status: {StatusCode}, Message: {Message}",

                dto.Id, result.StatusCode, result.Message);



        return StatusCode(result.StatusCode, result);

    }



    [Authorize(Roles = nameof(UserRole.Manager))]

    [HttpDelete("{Id}")]

    public async Task<ActionResult> DeleteItem(Guid Id)

    {

        _logger.LogInformation("DeleteProduct requested for {ProductId}", Id);

        var result = await _productService.DeleteProduct(Id);



        if (!result.Success)

            _logger.LogWarning("DeleteProduct failed for {ProductId}. Status: {StatusCode}, Message: {Message}",

                Id, result.StatusCode, result.Message);

        else

            _logger.LogInformation("DeleteProduct succeeded for {ProductId}", Id);



        return StatusCode(result.StatusCode, result);

    }

}


