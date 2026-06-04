using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Models;
public class User : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Client;
    public string? ImageUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime DateOfBirth { get; set; }
    public Sex Gender {  get; set; }
    public DateTime? LastLogin { get; set; }
    public ICollection<Ticket>? TicketsCreated { get; set; }
    public ICollection<Ticket>? TicketsAssigned { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}
