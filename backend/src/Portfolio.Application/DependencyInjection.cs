using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.Application.Dto;
using Portfolio.Application.Mapping;
using Portfolio.Application.Services;
using Portfolio.Application.Validators;

namespace Portfolio.Application;

/// <summary>
/// Composition root for the Application layer: manual AutoMapper registration
/// (no assembly scanning — explicit and warning-free) plus explicit validator
/// and service registrations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Manual registration (no assembly scanning, no extra DI package):
        // AutoMapper 15 requires an ILoggerFactory in MapperConfiguration.
        services.AddSingleton<IConfigurationProvider>(_ => new MapperConfiguration(
            cfg => cfg.AddProfile<PortfolioMappingProfile>(),
            NullLoggerFactory.Instance));
        services.AddScoped<IMapper>(sp => new Mapper(
            sp.GetRequiredService<IConfigurationProvider>(),
            sp.GetService));

        services.AddScoped<IValidator<CreateContactMessageRequest>, CreateContactMessageRequestValidator>();
        services.AddScoped<IValidator<CreateChatRequest>, CreateChatRequestValidator>();

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<IContactService, ContactService>();

        // RAG assistant. IEmbeddingService / IChatCompletionService are
        // implemented in Infrastructure and registered there; AiAssistantOptions
        // is bound from the "Ai" section by the API composition root.
        services.AddScoped<IChatService, AiChatService>();
        services.AddScoped<IPortfolioIndexer, ContentIndexer>();

        return services;
    }
}
