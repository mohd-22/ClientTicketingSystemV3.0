using AutoMapper;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.AuthDtos;
using ClientTicketingSystem.CORE.Dtos.CommentDtos;
using ClientTicketingSystem.CORE.Dtos.ProductDtos;
using ClientTicketingSystem.CORE.Dtos.TicketDtos;
using ClientTicketingSystem.CORE.Dtos.UserDtos;
using ClientTicketingSystem.CORE.Models;

namespace ClientTicketingSystem.Application.Mappings;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin));

        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.FullName : string.Empty))
            .ForMember(dest => dest.AssignedEmpName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : string.Empty))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<UserRegistraionDto, User>()
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Comment, CommentReadDto>()
            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.CommentText))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.ImageUrl : null))
            .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Role.ToString() : string.Empty))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.CreatorId));

        CreateMap<Attachment, AttachmentDto>();

        CreateMap<Product, ProductWithCountDto>();
    }
}