using AutoMapper;
using Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests.DTOs;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.WebApi.Endpoints.CrudEndpoints.IntegrationTests;

public class EntityToDtoProfile : Profile
{
    public EntityToDtoProfile()
    {
        CreateMap<Blog, BlogDto>().ReverseMap();
        CreateMap<BlogPost, BlogPostDto>().ReverseMap();
        CreateMap<BlogPostCategory, BlogPostCategoryDto>().ReverseMap();
        CreateMap<BlogPostTag, BlogPostTagDto>().ReverseMap();
    }
}
