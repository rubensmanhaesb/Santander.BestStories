using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Santander.BestStories.Application.Abstractions;
using Santander.BestStories.Application.Options;
using Santander.BestStories.Infrastructure.Clients;
using System.Net;

namespace Santander.BestStories.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private const string HackerNewsPolicyName = "HackerNewsPolicy";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<HackerNewsOptions>()
            .Bind(configuration.GetSection(HackerNewsOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IAsyncPolicy<HttpResponseMessage>>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<HackerNewsClient>>();

            var retry = BuildRetryPolicy(opt, logger);

            var breaker = BuildCircuitBreakerPolicy(opt, logger);

            return Policy.WrapAsync(breaker, retry);
        });

        services.AddHttpClient<IHackerNewsClient, HackerNewsClient>((sp, http) =>
        {
            var opt = sp.GetRequiredService<IOptions<HackerNewsOptions>>().Value;

            http.BaseAddress = new Uri(EnsureTrailingSlash(opt.BaseUrl), UriKind.Absolute);
            http.Timeout = opt.Timeout;
        })

        .AddPolicyHandler((IServiceProvider sp, HttpRequestMessage _) =>
            sp.GetRequiredService<IAsyncPolicy<HttpResponseMessage>>());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy(
        HackerNewsOptions opt,
        ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() 
            .OrResult(r => r.StatusCode == HttpStatusCode.Unauthorized)
            .OrResult(r => r.StatusCode == HttpStatusCode.Forbidden)
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .OrResult(r => r.StatusCode == HttpStatusCode.NotFound)
            .OrResult(r => r.StatusCode == HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(
                retryCount: opt.RetryCount,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(opt.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1)),
                onRetry: (outcome, delay, attempt, _) =>
                {
                    var reason = outcome.Exception?.GetType().Name
                                 ?? ((int)outcome.Result!.StatusCode).ToString();

                    logger.LogWarning(
                        "HN retry {Attempt}/{Max} in {Delay}ms. Reason={Reason}",
                        attempt, opt.RetryCount, delay.TotalMilliseconds, reason);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildCircuitBreakerPolicy(
        HackerNewsOptions opt,
        ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == HttpStatusCode.Unauthorized)
            .OrResult(r => r.StatusCode == HttpStatusCode.Forbidden)
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .OrResult(r => r.StatusCode == HttpStatusCode.NotFound)
            .OrResult(r => r.StatusCode == HttpStatusCode.BadRequest)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: opt.CircuitBreakerFailureThreshold,
                durationOfBreak: opt.CircuitBreakerBreakDuration,
                onBreak: (outcome, breakDelay) =>
                {
                    var reason = outcome.Exception?.GetType().Name
                                 ?? ((int)outcome.Result!.StatusCode).ToString();

                    logger.LogError(
                        "HN circuit OPEN for {BreakSeconds}s. Reason={Reason}",
                        breakDelay.TotalSeconds, reason);
                },
                onReset: () => logger.LogInformation("HN circuit CLOSED (reset)"),
                onHalfOpen: () => logger.LogWarning("HN circuit HALF-OPEN (trial request)")
            );
    }

    private static string EnsureTrailingSlash(string url)
        => url.EndsWith("/", StringComparison.Ordinal) ? url : url + "/";
}
