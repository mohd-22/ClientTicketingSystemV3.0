using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SupportHub.DATA.Repositories.Interfaces;
namespace ClientTicketingSystem.Application.Services;
public class AttachmentService : IAttachmentService
{
    private readonly IWebHostEnvironment _env;
    private readonly IUnitOfWork _unitOfWork;
    public AttachmentService(IWebHostEnvironment env, IUnitOfWork unitOfWork)
    {
        _env = env;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ApiResponse<bool>> UploadAttachment(Guid userId, Guid ticketId, IFormFile file)
    {
        if (file == null)
        {
            return new ApiResponse<bool> { Data = false, Message = "File is Required", Success = false, StatusCode = 400 };
        }
        try
        {
            string uploadFolder = Path.Combine(_env.WebRootPath, "Attachments");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var attachment = new Attachment()
            {
                FileName = fileName,
                FilePath = Path.Combine("Attachments", fileName),
                TicketId = ticketId,
                CreatedBy = userId

            };
            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<bool> { Data = true, Message = "Attachment uploaded successfully", Success = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to upload attachment", ex);
        }
    }
    public async Task<ApiResponse<IEnumerable<AttachmentDto>>> GetAttachmentsByTicket(Guid ticketId)
    {
        var attachments = await _unitOfWork.Attachments.FindAsNoTrackingAsync(a => a.TicketId == ticketId);
        if (attachments == null)
        {
            return new ApiResponse<IEnumerable<AttachmentDto>> { Data = null, Message = "Attachments not found", Success = false, StatusCode = 404 };
            
        }
        var attachmentDtos = attachments.Select(a => new AttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            FilePath = a.FilePath,
        }).ToList();
        return new ApiResponse<IEnumerable<AttachmentDto>> { Data = attachmentDtos, Message = "Attachments Fetched Successfully", Success = true, StatusCode = 200 };
        
    }
    public async Task<(byte[] fileBytes, string contentType, string fileName)> DownloadFile(Guid id)
    {
        var attachment = await _unitOfWork.Attachments.GetByIdAsync(id);
        if (attachment == null) return (null!, null!, null!);

        var fullPath = Path.Combine(_env.WebRootPath, attachment.FilePath);
        if (!System.IO.File.Exists(fullPath)) return (null!, null!, null!);

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return (fileBytes, "application/octet-stream", attachment.FileName);
    }
}
