using AutoMapper;
using Portfolio.Application.Dto;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Mapping;

/// <summary>
/// Entity → DTO mappings. DTOs are records whose constructor parameters match
/// entity property names, so convention mapping applies; only the contact
/// request needs explicit member rules (identity/audit fields are set by the service).
/// Mapping choice (AutoMapper, manual registration) is documented in ADR-0002.
/// </summary>
public sealed class PortfolioMappingProfile : Profile
{
    public PortfolioMappingProfile()
    {
        CreateMap<Project, ProjectDto>();
        CreateMap<BlogPost, BlogPostDto>();
        CreateMap<Skill, SkillDto>();
        CreateMap<Experience, ExperienceDto>();

        CreateMap<CreateContactMessageRequest, ContactMessage>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAtUtc, o => o.Ignore())
            .ForMember(d => d.UpdatedAtUtc, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.UpdatedBy, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.IsHandled, o => o.Ignore())
            .ForMember(d => d.IpHash, o => o.Ignore());
    }
}
