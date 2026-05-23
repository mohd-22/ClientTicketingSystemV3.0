using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.AuthDtos;
using ClientTicketingSystem.CORE.Dtos.UserDtos;
using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;
using ClientTicketingSystem.CORE.Specifications;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;

namespace ClientTicketingSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    private readonly IWebHostEnvironment _env;


    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _env = env;
    }
    public async Task<ApiResponse<bool>> ActivateUserAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return new ApiResponse<bool> { Data = false, Message = "User Not found", Success = false, StatusCode = 404 };
        if (user.IsActive == true) { return new ApiResponse<bool> { Data = false, Message = "User is already Active", Success = false, StatusCode = 400 }; }

        user.IsActive = true;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();
        return new ApiResponse<bool> { Data = true, Message = "user Activated Succesfully", Success = true, StatusCode = 200 };
    }
    public async Task<ApiResponse<bool>> DeactivateUserAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return new ApiResponse<bool> { Data = false, Message = "User Not found", Success = false, StatusCode = 404 };
        if (user.IsActive == false) { return new ApiResponse<bool> { Data = false, Message = "User is already Deactivated", Success = false, StatusCode = 400 }; }

        bool canDeactivate = true;

        if (user.Role == UserRole.Client)
        {
            canDeactivate = await HandleClientDeactivation(id);
        }
        else if (user.Role == UserRole.Employee)
        {
            canDeactivate = await HandleEmployeeDeactivation(id);
        }

        if (!canDeactivate) return new ApiResponse<bool> { Data = false, Message = "The Client has incomplete requests yet.", Success = false, StatusCode = 400 };

        user.IsActive = false;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();
        return new ApiResponse<bool> { Data = true, Message = "user Deactivated Succesfully", Success = true, StatusCode = 200 };

    }
    private async Task<bool> HandleEmployeeDeactivation(Guid clientId)
    {
        var requests = await _unitOfWork.Tickets.FindAllAsync(r =>

            r.AssignedEmpId == clientId &&
            r.Status != TicketStatus.Closed);

        foreach (var req in requests)
        {
            req.Status = TicketStatus.Paused;
            req.AssignedEmpId = null;
            _unitOfWork.Tickets.Update(req);
        }
        return true;
    }
    private async Task<bool> HandleClientDeactivation(Guid employeeId)
    {

        bool hasActiveRequests = await _unitOfWork.Tickets.AnyAsync(r => r.CreatedBy == employeeId );
        
        return !hasActiveRequests;
    }
    public async Task<ApiResponse<PaginationDto<UserDto>>> GetAllUsersAsync(
        string? search,
        string? sort,
        UserRole? role,
        bool? isActive,
        int pageIndex,
        int pageSize)
    {
        try
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var spec = new UsersWithFiltersSpecification(search, sort, role, isActive, pageIndex, pageSize);
            var countSpec = new UsersWithFiltersForCountSpecification(search, role, isActive);

            var users = await _unitOfWork.Users.ListWithSpecAsync(spec);
            var totalCount = await _unitOfWork.Users.CountAsync(countSpec);

            var userDtos = users.Select(MapToUserDto).ToList();
            var pagedResult = new PaginationDto<UserDto>(pageIndex, pageSize, totalCount, userDtos);

            _logger.LogInformation(
                "Retrieved {UserCount} of {TotalCount} users (page {PageIndex}, size {PageSize})",
                userDtos.Count, totalCount, pageIndex, pageSize);

            return new ApiResponse<PaginationDto<UserDto>>
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = pagedResult,
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return new ApiResponse<PaginationDto<UserDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving users",
                StatusCode = 500
            };
        }
    }

    private static UserDto MapToUserDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        UserName = user.UserName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Address = user.Address,
        ImageUrl = user.ImageUrl,
        DateOfBirth = user.DateOfBirth,
        Gender = user.Gender,
        Role = user.Role,
        IsActive = user.IsActive
        ,
        CreatedAt = user.CreatedDate,
        LastLogin = user.LastLogin
    };
    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id)
    {

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return new ApiResponse<UserDto> { Data = null, Message = "User Not found", Success = false, StatusCode = 404 };
        var userDto = MapToUserDto(user!);
        return new ApiResponse<UserDto> { Data = userDto, Message = "User Retrieved Succesfully", Success = true, StatusCode = 200 };
    }
    public async Task<ApiResponse<UserRegistraionDto>> CreateUserAsync(UserRegistraionDto request, Guid UserId)
    {
        if (await _unitOfWork.Users.AnyAsync(u => u.UserName == request.UserName))
        {
            return new ApiResponse<UserRegistraionDto>
            {
                Success = false,
                Message = "Username already exists",
                StatusCode = 400
            };
        }
        if (await _unitOfWork.Users.AnyAsync(u => u.Email == request.Email))
        {

            return new ApiResponse<UserRegistraionDto>
            {
                Success = false,
                Message = "Email already exists",
                StatusCode = 400
            };
        }

        if (await _unitOfWork.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
        {
            return new ApiResponse<UserRegistraionDto>
            {
                Success = false,
                Message = "Phone number already exists",
                StatusCode = 400
            };
        }

        var user = new User
        {
            UserName = request.UserName,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            DateOfBirth = request.DateOfBirth,
            Role = UserRole.Employee,
            Gender = request.Gender,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = UserId
        };
        user.HashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();
        return new ApiResponse<UserRegistraionDto> { Message = "User Created succesfully", Success = true, StatusCode = 200 };
    }
    public async Task<ApiResponse<bool>> UpdtaeUserAsync(UpdateUserDto request, Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return new ApiResponse<bool> { Data = false, Message = "User Not found", Success = false, StatusCode = 404 };
        var cheakPhone = await _unitOfWork.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != id);

        if (cheakPhone) return new ApiResponse<bool> { Data = false, Message = "Phone number already exists", Success = false, StatusCode = 400 };
        try
        {
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            _logger.LogInformation("Updating user with ID {UserId}", id);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("User with ID {UserId} updated successfully", id);
            return new ApiResponse<bool> { Data = true, Message = "User Updated Successfully", Success = true, StatusCode = 200 };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            return new ApiResponse<bool> { Data = false, Message = "An error occurred while updating the user", Success = false, StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<bool>> ChangeAvatar(Guid userId, IFormFile file)
    {
        if (file == null)
        {
            return new ApiResponse<bool> { Data = false, Message = "File is Required", Success = false, StatusCode = 400 };
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return new ApiResponse<bool> { Data = false, Message = "User Not found", Success = false, StatusCode = 404 };
        }

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string uploadFolder = Path.Combine(_env.WebRootPath, "Attachments");
        string filePath = Path.Combine(uploadFolder, fileName);

        try
        {
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ImageUrl = Path.Combine("Attachments", fileName);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return new ApiResponse<bool> { Data = true, Message = "Avatar changed successfully", Success = true, StatusCode = 200 };
        }
        catch (Exception ex)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogWarning("Cleaned up uploaded file due to database failure: {FilePath}", filePath);
            }

            _logger.LogError(ex, "Error occurred while changing avatar for user {UserId}", userId);

            throw;
        }
    }

    public async Task<ApiResponse<bool>> AssignTicketToEmployee(Guid TicketId, Guid EmployeeId)
    {
        try
        {
            _logger.LogInformation("Assigning a ticket to an employee.");
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(TicketId);
            if (ticket == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };

            var employee = await _unitOfWork.Users.GetByIdAsync(EmployeeId);
            if (employee == null) return new ApiResponse<bool> { Data = false, Message = "Employee Not found", Success = false, StatusCode = 404 };

            if (!employee.IsActive) return new ApiResponse<bool> { Data = false, Message = "Employee is not active", Success = false, StatusCode = 400 };

            if (ticket.AssignedEmpId != null) return new ApiResponse<bool> { Data = false, Message = "Ticket already assigned to an employee", Success = false, StatusCode = 400 };

            if (ticket.Status == TicketStatus.New || ticket.Status == TicketStatus.Paused)
            {
                ticket.Status = TicketStatus.Assigned;
                ticket.AssignedEmpId = EmployeeId;
                _unitOfWork.Tickets.Update(ticket);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Ticket assigned successfully.");
                return new ApiResponse<bool> { Data = true, Message = "Ticket Assigned Successfully", Success = true, StatusCode = 200 };
            }
            _logger.LogWarning("you cant assign staff to a request unless it 'New' or 'Paused' ");
            return new ApiResponse<bool> { Data = false, Message = "you cant assign staff to a request unless it 'New' or 'Paused' ", Success = false, StatusCode = 400 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while assigning a ticket to an employee.");
            return new ApiResponse<bool> { Data = false, Message = "An error occurred while assigning a ticket to an employee.", Success = false, StatusCode = 500 };
        }
    }
    public async Task<ApiResponse<bool>> TicketChangeStatus(Guid TicketId)
    {
        try
        {
            _logger.LogInformation("Changing the status of the ticket.");
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(TicketId);
            if (ticket == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };
            if (ticket.Status == TicketStatus.Assigned)
            {
                ticket.Status = TicketStatus.InProgress;
                _unitOfWork.Tickets.Update(ticket);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Ticket status changed successfully.");
                return new ApiResponse<bool> { Data = true, Message = "Ticket Updated Successfully", Success = true, StatusCode = 200 };
            }
            else if (ticket.Status == TicketStatus.InProgress)
            {
                if (ticket.IsFixed == false) return new ApiResponse<bool> { Data = false, Message = "Ticket is not fixed yet", Success = false, StatusCode = 400 };
                ticket.Status = TicketStatus.Closed;
                _unitOfWork.Tickets.Update(ticket);
                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Ticket status changed successfully.");

                return new ApiResponse<bool> { Data = true, Message = "Ticket Updated Successfully", Success = true, StatusCode = 200 };
            }
            _logger.LogWarning("you cant change the status of a request unless it 'Assigned' or 'InProgress' ");
            return new ApiResponse<bool> { Data = false, Message = "you cant change the status of a request unless it 'Assigned' or 'InProgress' ", Success = false, StatusCode = 400 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the status of the ticket.");
            return new ApiResponse<bool> { Data = false, Message = "An error occurred while changing the status of the ticket.", Success = false, StatusCode = 500 };
        }
    }
}

