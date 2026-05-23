using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos.AuthDtos;
using ClientTicketingSystem.CORE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SupportHub.DATA.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClientTicketingSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> LoginAsync(LoginDto request)
    {
        try
        {
            var user = await _unitOfWork.Users.FindAsync(u =>
                u.UserName == request.EmailOrUsername || u.Email == request.EmailOrUsername || u.PhoneNumber == request.EmailOrUsername);

            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found for {LoginIdentifier}", request.EmailOrUsername);
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Wrong credentials",
                    StatusCode = 401
                };
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.HashedPassword, request.Password) ==
                PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Login failed: invalid password for user {UserId}", user.Id);
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Wrong credentials",
                    StatusCode = 401
                };
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: inactive user {UserId}", user.Id);
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "User is not active",
                    StatusCode = 400
                };
            }

            _logger.LogInformation("User {UserId} authenticated successfully", user.Id);
            user.LastLogin = DateTime.UtcNow;
            _logger.LogInformation("User {UserId} logged in at {LoginTime}", user.Id, user.LastLogin);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<string>
            {
                Success = true,
                Message = "Login successful",
                StatusCode = 200,
                Data = CreateToken(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for {LoginIdentifier}", request.EmailOrUsername);
            return new ApiResponse<string>
            {
                Success = false,
                Message = "An error occurred while processing the login request",
                StatusCode = 500
            };
        }
    }

    public async Task<ApiResponse<UserRegistraionDto>> RigisterUserAsync(UserRegistraionDto request)
    {
        try
        {
            if (await _unitOfWork.Users.AnyAsync(u => u.UserName == request.UserName))
            {
                _logger.LogWarning("Registration failed: username {UserName} already exists", request.UserName);
                return new ApiResponse<UserRegistraionDto>
                {
                    Success = false,
                    Message = "User name already exists",
                    StatusCode = 400
                };
            }

            if (await _unitOfWork.Users.AnyAsync(u => u.Email == request.Email))
            {
                _logger.LogWarning("Registration failed: email {Email} already exists", request.Email);
                return new ApiResponse<UserRegistraionDto>
                {
                    Success = false,
                    Message = "Email already exists",
                    StatusCode = 400
                };
            }

            if (await _unitOfWork.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
            {
                _logger.LogWarning("Registration failed: phone number already exists for {UserName}", request.UserName);
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
                Gender = request.Gender,
                CreatedDate = DateTime.UtcNow
            };
            user.HashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User registered successfully with id {UserId}", user.Id);
            return new ApiResponse<UserRegistraionDto>
            {
                Success = true,
                Message = "User created successfully",
                StatusCode = 200,
                Data = request
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for {UserName}", request.UserName);
            return new ApiResponse<UserRegistraionDto>
            {
                Success = false,
                Message = "An error occurred while processing the registration request",
                StatusCode = 500
            };
        }
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FullName", user.FullName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JwtSettings:SecretKey")!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("JwtSettings:Issuer"),
            audience: _configuration.GetValue<string>("JwtSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
