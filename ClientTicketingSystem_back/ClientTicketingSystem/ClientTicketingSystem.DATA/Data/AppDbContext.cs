using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClientTicketingSystem.DATA.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductModule> ProductModules { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Ticket>(b =>
        {
            b.HasOne(t => t.Client)
             .WithMany(u => u.TicketsCreated)
             .HasForeignKey(t => t.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);


            b.HasOne(t => t.AssignedUser)
             .WithMany(u => u.TicketsAssigned)
             .HasForeignKey(t => t.AssignedEmpId)
                  .OnDelete(DeleteBehavior.Restrict);


        });

        modelBuilder.Entity<Comment>(b =>
        {
            b.Property(c => c.CommentText).IsRequired();
            b.HasOne(c => c.Creator)
             .WithMany(u => u.Comments)
             .HasForeignKey(c => c.CreatedBy);
        });

        modelBuilder.Entity<User>().HasData(
         new User
         {
             Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
             FullName = "Moahmmed Alassi",
             UserName = "admin",
             Email = "admin@supporthub.com",
             PhoneNumber = "0799999999",
             Address = "Amman, Jordan",
             HashedPassword = "AQAAAAIAAYagAAAAEKkB6aXFw8CerrUrN0OsWO0pBbCJt/mSGfsTJ9XMP0kCkUiuUZbTHez2JbMQ36JSLA==",
             Role = UserRole.Manager,
             IsActive = true,
             DateOfBirth = new DateTime(1995, 5, 10),
             Gender = Sex.Male,
             CreatedDate = DateTime.Now
         }
       );
    }
}
