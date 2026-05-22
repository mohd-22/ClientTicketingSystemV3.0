using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos;
using Microsoft.AspNetCore.Http;
namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IAttachmentService
{
    Task<(byte[] fileBytes, string contentType, string fileName)> DownloadFile(Guid id);
    Task<ApiResponse<IEnumerable<AttachmentDto>>> GetAttachmentsByTicket(Guid ticketId);
    Task<ApiResponse<bool>> UploadAttachment(Guid userId, Guid ticketId, IFormFile file);
}
