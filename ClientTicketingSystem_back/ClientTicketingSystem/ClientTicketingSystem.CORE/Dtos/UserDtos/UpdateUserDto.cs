using ClientTicketingSystem.CORE.Models.Enums;
namespace ClientTicketingSystem.CORE.Dtos.UserDtos;
public class UpdateUserDto
{    

    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Sex Gender { get; set; }
}
