namespace Santander.BestStories.Application.Options;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    public string BaseUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    // Cache
    public TimeSpan BestStoriesCacheTtl { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ItemCacheTtl { get; set; } = TimeSpan.FromMinutes(10);

    // Controle de concorrência (por request)
    public int MaxConcurrentRequests { get; set; } = 16;
}
