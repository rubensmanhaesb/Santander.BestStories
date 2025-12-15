namespace Santander.BestStories.Application.Options;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    // Cache
    public TimeSpan BestStoriesCacheTtl { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ItemCacheTtl { get; set; } = TimeSpan.FromMinutes(10);

    // Limites/Perf
    public int MaxConcurrentRequests { get; set; } = 16;

    // Pool (para pegar mais IDs que N e reduzir chamadas perdidas)
    public int PoolMultiplier { get; set; } = 3;
    public int PoolMax { get; set; } = 500;

    // Limite do endpoint
    public int MaxN { get; set; } = 200;
}
