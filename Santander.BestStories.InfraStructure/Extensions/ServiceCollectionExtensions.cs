using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Infrastructure.Clients;
using Santander.BestStories.Infrastructure.Options;

namespace Santander.BestStories.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HackerNewsOptions>(
            configuration.GetSection(HackerNewsOptions.SectionName));

        services.AddHttpClient<IHackerNewsClient, HackerNewsClient>((sp, http) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<HackerNewsOptions>>().Value;

            http.BaseAddress = new Uri(EnsureTrailingSlash(options.BaseUrl));
            http.Timeout = options.Timeout;
        });

        return services;
    }

    private static string EnsureTrailingSlash(string url)
        => url.EndsWith("/", StringComparison.Ordinal) ? url : url + "/";
}
