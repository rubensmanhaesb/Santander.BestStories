namespace Santander.BestStories.Infrastructure.Options;

public sealed class HackerNewsOptions
{
    public const string SectionName = "HackerNews";

    /// <summary>
    /// Base URL of the Hacker News Firebase API.
    /// Example: https://hacker-news.firebaseio.com/v0/
    /// </summary>
    public string BaseUrl { get; init; } = "https://hacker-news.firebaseio.com/v0/";

    /// <summary>
    /// Timeout for HTTP calls to the external API.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5);
}
