using Microsoft.Extensions.DependencyInjection;
using Santander.BestStories.Application.Interfaces;
using Santander.BestStories.Application.Services;

namespace Santander.BestStories.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBestStoriesService, BestStoriesService>();
        return services;
    }
}
