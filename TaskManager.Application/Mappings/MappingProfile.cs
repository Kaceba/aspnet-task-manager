using AutoMapper;
using TaskManager.Core.DTOs.Auth;
using TaskManager.Core.DTOs.Categories;
using TaskManager.Core.DTOs.Tasks;
using TaskManager.Core.Entities;

namespace TaskManager.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        CreateMap<User, AuthResponse>()
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // Task mappings
        CreateMap<TaskRequest, TaskItem>()
            .ForMember(dest => dest.Tags, opt => opt.Ignore());

        CreateMap<TaskItem, TaskResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.Name).ToList()));

        // Category mappings
        CreateMap<CategoryRequest, Category>();

        CreateMap<Category, CategoryResponse>()
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count));
    }
}
