using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Dtos;
public class UserDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime DateOfBirth { get; set; }

    public Sex Gender { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}