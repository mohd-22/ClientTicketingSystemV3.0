using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;
namespace ClientTicketingSystem.Application.Services;
public class AttachmentService : IAttachmentService
{
    private readonly IWebHostEnvironment _env;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttachmentService> _logger;
    private readonly IMapper _mapper;

    public AttachmentService(IWebHostEnvironment env, IUnitOfWork unitOfWork, ILogger<AttachmentService> logger, IMapper mapper)
    {
        _env = env;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ApiResponse<bool>> UploadAttachment(Guid userId, Guid ticketId, IFormFile file)
    {
        if (file == null)
        {
            return new ApiResponse<bool> { Data = false, Message = "File is Required", Success = false, StatusCode = 400 };
        }
        try
        {
            _logger.LogInformation("Uploading attachment for ticket with ID: {TicketId}", ticketId);
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
            _logger.LogInformation("Attachment uploaded successfully for ticket with ID: {TicketId}", ticketId);
            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<bool> { Data = true, Message = "Attachment uploaded successfully", Success = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload attachment for ticket with ID: {TicketId}", ticketId);
            throw new Exception("Failed to upload attachment", ex);
        }
    }
    public async Task<ApiResponse<IEnumerable<AttachmentDto>>> GetAttachmentsByTicket(Guid ticketId)
    {
            var attachments = await _unitOfWork.Attachments.FindAsNoTrackingAsync(a => a.TicketId == ticketId);
        try
        {
            _logger.LogInformation("Getting attachments for ticket with ID: {TicketId}", ticketId);
            if (attachments == null)
            {
                return new ApiResponse<IEnumerable<AttachmentDto>> { Data = null, Message = "Attachments not found", Success = false, StatusCode = 404 };

            }
           
            var attachmentDtos = _mapper.Map<List<AttachmentDto>>(attachments);
            return new ApiResponse<IEnumerable<AttachmentDto>> { Data = attachmentDtos, Message = "Attachments Fetched Successfully", Success = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get attachments for ticket with ID: {TicketId}", ticketId);
            throw new Exception("Failed to get attachments", ex);
        }

    }
    public async Task<(byte[] fileBytes, string contentType, string fileName)> DownloadFile(Guid id)
    {
        var attachment = await _unitOfWork.Attachments.GetByIdAsync(id);
        if (attachment == null) return (null!, null!, null!);
        try
        {
            _logger.LogInformation("Downloading attachment with ID: {AttachmentId}", id);
            var fullPath = Path.Combine(_env.WebRootPath, attachment.FilePath);
            if (!System.IO.File.Exists(fullPath)) return (null!, null!, null!);

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return (fileBytes, "application/octet-stream", attachment.FileName);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to download attachment with ID: {AttachmentId}", id);
            throw new Exception("Failed to download attachment", ex);
        }
    }
}
