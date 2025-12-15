namespace Santander.BestStories.Application.Options;

public class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    public string BaseUrl { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    // Circuit Breaker
    public int CircuitBreakerFailureThreshold { get; set; } = 3;
    public TimeSpan CircuitBreakerBreakDuration { get; set; } = TimeSpan.FromSeconds(30);

    // Retry
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    // Cache
    public TimeSpan BestStoriesCacheTtl { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ItemCacheTtl { get; set; } = TimeSpan.FromMinutes(10);

    // Parallelism
    public int MaxDegreeOfParallelism { get; set; } = 16;
    public int PoolMultiplier { get; set; } = 3;
    public int PoolMax { get; set; } = 500;
    public int MaxN { get; set; } = 200;
    public int MaxConcurrentRequests { get; set; } = 16;
}

