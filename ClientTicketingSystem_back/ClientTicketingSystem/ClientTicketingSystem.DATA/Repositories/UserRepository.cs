using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.DATA.Data;
using SupportHub.DATA.Repositories.Interfaces;

namespace SupportHub.DATA.Repositories;
public class UserRepository : GenericRepository<User>,IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

}